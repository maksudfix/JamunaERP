using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.Sales;

public class CreateSaleCommandHandlerTests
{
    private static CreateSaleCommand CashSale(
        int customerId, int productId, int qty, decimal paid,
        StockLocationType location = StockLocationType.Showroom1)
        => new(customerId, location, paid, false, 0, null,
               new List<CreateSaleItemInput> { new(productId, qty) });

    private static CreateSaleCommand InstallmentSale(
        int customerId, int productId, int qty, decimal paid, int count,
        DateTime? firstDue = null, StockLocationType location = StockLocationType.Showroom1)
        => new(customerId, location, paid, true, count, firstDue,
               new List<CreateSaleItemInput> { new(productId, qty) });

    // ---------- Happy paths ----------

    [Fact]
    public async Task CashSale_CreatesSaleWithItems_AndDeductsStock()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        var saleId = await handler.Handle(CashSale(customer.Id, product.Id, 3, 3000m), CancellationToken.None);

        var sale = await db.Sales.AsNoTracking()
            .Include(s => s.Items).Include(s => s.Installments)
            .SingleAsync(s => s.Id == saleId);

        Assert.StartsWith("INV-", sale.InvoiceNo);
        Assert.Equal(3000m, sale.TotalAmount);
        Assert.Equal(3000m, sale.PaidAmount);
        Assert.False(sale.IsInstallment);
        Assert.Empty(sale.Installments);

        var item = Assert.Single(sale.Items);
        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(1000m, item.UnitPrice); // price comes from the product, not the client

        var stock = await db.StockItems.AsNoTracking().SingleAsync();
        Assert.Equal(7, stock.Quantity);
    }

    [Fact]
    public async Task DuplicateProductLines_AreMergedIntoOneSaleItem()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        var command = new CreateSaleCommand(customer.Id, StockLocationType.Showroom1, 3000m, false, 0, null,
            new List<CreateSaleItemInput> { new(product.Id, 1), new(product.Id, 2) });

        var saleId = await handler.Handle(command, CancellationToken.None);

        var sale = await db.Sales.AsNoTracking().Include(s => s.Items).SingleAsync(s => s.Id == saleId);
        var item = Assert.Single(sale.Items);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(7, (await db.StockItems.AsNoTracking().SingleAsync()).Quantity);
    }

    [Fact]
    public async Task Stock_IsDeductedFifoAcrossRows()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 3);   // older row
        await TestDb.AddStockAsync(db, product.Id, 5);   // newer row
        var handler = new CreateSaleCommandHandler(db);

        await handler.Handle(CashSale(customer.Id, product.Id, 6, 6000m), CancellationToken.None);

        var rows = await db.StockItems.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        Assert.Equal(new[] { 0, 2 }, rows.Select(r => r.Quantity).ToArray());
    }

    // ---------- Installment generation ----------

    [Fact]
    public async Task InstallmentSale_GeneratesMonthlySchedule_ForTheOutstandingBalance()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 5);
        var handler = new CreateSaleCommandHandler(db);
        var firstDue = new DateTime(2027, 1, 15);

        var saleId = await handler.Handle(
            InstallmentSale(customer.Id, product.Id, 1, paid: 100m, count: 3, firstDue: firstDue),
            CancellationToken.None);

        var sale = await db.Sales.AsNoTracking().Include(s => s.Installments).SingleAsync(s => s.Id == saleId);
        var plans = sale.Installments.OrderBy(i => i.InstallmentNumber).ToList();

        Assert.True(sale.IsInstallment);
        Assert.Equal(3, sale.InstallmentCount);
        Assert.Equal(new[] { 1, 2, 3 }, plans.Select(p => p.InstallmentNumber).ToArray());
        Assert.Equal(new[] { 300m, 300m, 300m }, plans.Select(p => p.AmountDue).ToArray());
        Assert.Equal(
            new[] { new DateTime(2027, 1, 15), new DateTime(2027, 2, 15), new DateTime(2027, 3, 15) },
            plans.Select(p => p.DueDate).ToArray());
        Assert.All(plans, p => Assert.Equal(InstallmentStatus.Pending, p.Status));
        Assert.Equal(900m, plans.Sum(p => p.AmountDue)); // total 1000 - paid 100
    }

    [Fact]
    public async Task InstallmentSale_PutsRoundingRemainderOnTheLastInstallment()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 5);
        var handler = new CreateSaleCommandHandler(db);

        var saleId = await handler.Handle(
            InstallmentSale(customer.Id, product.Id, 1, paid: 0m, count: 3),
            CancellationToken.None);

        var plans = await db.InstallmentPlans.AsNoTracking()
            .Where(p => p.SaleId == saleId).OrderBy(p => p.InstallmentNumber).ToListAsync();

        Assert.Equal(new[] { 333.33m, 333.33m, 333.34m }, plans.Select(p => p.AmountDue).ToArray());
        Assert.Equal(1000m, plans.Sum(p => p.AmountDue)); // no money lost to rounding
    }

    // ---------- Stock rules ----------

    [Fact]
    public async Task InsufficientStock_Throws_AndLeavesStockUnchanged()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 2);
        var handler = new CreateSaleCommandHandler(db);

        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
            handler.Handle(CashSale(customer.Id, product.Id, 5, 5000m), CancellationToken.None));

        Assert.Contains("available 2", ex.Message);
        Assert.Equal(2, (await db.StockItems.AsNoTracking().SingleAsync()).Quantity);
        Assert.Empty(await db.Sales.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task MultiLineSale_WithOneShortLine_SavesNothing()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var productA = await TestDb.AddProductAsync(db, name: "Product A");
        var productB = await TestDb.AddProductAsync(db, name: "Product B");
        await TestDb.AddStockAsync(db, productA.Id, 5);
        await TestDb.AddStockAsync(db, productB.Id, 1);
        var handler = new CreateSaleCommandHandler(db);

        var command = new CreateSaleCommand(customer.Id, StockLocationType.Showroom1, 5000m, false, 0, null,
            new List<CreateSaleItemInput> { new(productA.Id, 2), new(productB.Id, 3) });

        await Assert.ThrowsAsync<InsufficientStockException>(() => handler.Handle(command, CancellationToken.None));

        var stock = await db.StockItems.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        Assert.Equal(new[] { 5, 1 }, stock.Select(s => s.Quantity).ToArray());
        Assert.Empty(await db.Sales.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task FaultyStock_IsNotSellable()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 10, isFaulty: true);
        await TestDb.AddStockAsync(db, product.Id, 1);
        var handler = new CreateSaleCommandHandler(db);

        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
            handler.Handle(CashSale(customer.Id, product.Id, 2, 2000m), CancellationToken.None));

        Assert.Contains("available 1", ex.Message);
    }

    [Fact]
    public async Task StockInAnotherLocation_IsNotSellable()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 10, StockLocationType.GoDown);
        var handler = new CreateSaleCommandHandler(db);

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            handler.Handle(CashSale(customer.Id, product.Id, 1, 1000m, StockLocationType.Showroom1),
                CancellationToken.None));
    }

    // ---------- Payment rules ----------

    [Fact]
    public async Task CashSale_NotPaidInFull_Throws_AndLeavesStockUnchanged()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(CashSale(customer.Id, product.Id, 1, 500m), CancellationToken.None));

        Assert.Equal(10, (await db.StockItems.AsNoTracking().SingleAsync()).Quantity);
        Assert.Empty(await db.Sales.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task PaidAmount_ExceedingTotal_Throws()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(CashSale(customer.Id, product.Id, 1, 1500m), CancellationToken.None));
    }

    [Fact]
    public async Task InstallmentSale_FullyPaidUpFront_Throws()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(InstallmentSale(customer.Id, product.Id, 1, paid: 1000m, count: 3),
                CancellationToken.None));
    }

    // ---------- Lookups ----------

    [Fact]
    public async Task UnknownCustomer_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(CashSale(customerId: 999, product.Id, 1, 1000m), CancellationToken.None));
    }

    [Fact]
    public async Task UnknownProduct_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var handler = new CreateSaleCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(CashSale(customer.Id, productId: 999, 1, 1000m), CancellationToken.None));
    }
}