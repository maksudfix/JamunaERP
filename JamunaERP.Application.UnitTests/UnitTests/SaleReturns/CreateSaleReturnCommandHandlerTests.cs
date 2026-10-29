using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn;
using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using JamunaERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.SaleReturns;

public class CreateSaleReturnCommandHandlerTests
{
    // Creates stock (10 units), then sells 'qty' units at 1000 each
    private static async Task<(int SaleId, int SaleItemId, int ProductId)> SellAsync(
        AppDbContext db, int qty, decimal paid, bool installment = false, int count = 0)
    {
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);

        var command = new CreateSaleCommand(
            customer.Id, StockLocationType.Showroom1, paid, installment, count, null,
            new List<CreateSaleItemInput> { new(product.Id, qty) });

        var saleId = await new CreateSaleCommandHandler(db).Handle(command, CancellationToken.None);
        var saleItem = await db.SaleItems.AsNoTracking().SingleAsync(i => i.SaleId == saleId);

        return (saleId, saleItem.Id, product.Id);
    }

    private static CreateSaleReturnCommand Return(
        int saleId, int saleItemId, int qty, int faulty = 0,
        StockLocationType location = StockLocationType.Showroom1)
        => new(saleId, "Customer changed mind", location,
               new List<SaleReturnLineInput> { new(saleItemId, qty, faulty) });

    [Fact]
    public async Task CashSale_Return_RefundsFullValue_AndRestocks()
    {
        using var db = TestDb.Create();
        var (saleId, saleItemId, _) = await SellAsync(db, qty: 3, paid: 3000m);
        var handler = new CreateSaleReturnCommandHandler(db);

        var result = await handler.Handle(Return(saleId, saleItemId, 1), CancellationToken.None);

        Assert.Equal(1000m, result.ReturnedValue);
        Assert.Equal(0m, result.OutstandingReduced);
        Assert.Equal(1000m, result.RefundAmount);

        var sale = await db.Sales.AsNoTracking().SingleAsync();
        Assert.Equal(1000m, sale.ReturnedAmount);
        Assert.Equal(1000m, sale.RefundedAmount);

        var stock = await db.StockItems.AsNoTracking().SingleAsync();
        Assert.Equal(8, stock.Quantity); // 10 - 3 sold + 1 returned
        Assert.False(stock.IsFaulty);
    }

    [Fact]
    public async Task FaultyUnits_GoToTheFaultyPool()
    {
        using var db = TestDb.Create();
        var (saleId, saleItemId, _) = await SellAsync(db, qty: 3, paid: 3000m);
        var handler = new CreateSaleReturnCommandHandler(db);

        await handler.Handle(Return(saleId, saleItemId, qty: 2, faulty: 1), CancellationToken.None);

        var good = await db.StockItems.AsNoTracking().SingleAsync(s => !s.IsFaulty);
        var faulty = await db.StockItems.AsNoTracking().SingleAsync(s => s.IsFaulty);
        Assert.Equal(8, good.Quantity);   // 7 left + 1 good return
        Assert.Equal(1, faulty.Quantity);
    }

    [Fact]
    public async Task InstallmentSale_Return_CancelsLastInstallmentsFirst_NoRefund()
    {
        using var db = TestDb.Create();
        // total 3000, paid 600 -> balance 2400 -> 3 installments of 800
        var (saleId, saleItemId, _) = await SellAsync(db, qty: 3, paid: 600m, installment: true, count: 3);
        var handler = new CreateSaleReturnCommandHandler(db);

        var result = await handler.Handle(Return(saleId, saleItemId, 1), CancellationToken.None);

        Assert.Equal(1000m, result.OutstandingReduced);
        Assert.Equal(0m, result.RefundAmount);

        var plans = await db.InstallmentPlans.AsNoTracking()
            .Where(p => p.SaleId == saleId).OrderBy(p => p.InstallmentNumber).ToListAsync();

        Assert.Equal(800m, plans[0].AmountDue);
        Assert.Equal(600m, plans[1].AmountDue);                       // 800 - 200
        Assert.Equal(InstallmentStatus.Cancelled, plans[2].Status);   // fully removed
    }

    [Fact]
    public async Task InstallmentSale_ReturnEverything_RefundsWhatWasPaid()
    {
        using var db = TestDb.Create();
        var (saleId, saleItemId, _) = await SellAsync(db, qty: 3, paid: 600m, installment: true, count: 3);
        var handler = new CreateSaleReturnCommandHandler(db);

        var result = await handler.Handle(Return(saleId, saleItemId, 3), CancellationToken.None);

        Assert.Equal(3000m, result.ReturnedValue);
        Assert.Equal(2400m, result.OutstandingReduced);
        Assert.Equal(600m, result.RefundAmount); // only the down payment goes back

        var plans = await db.InstallmentPlans.AsNoTracking().Where(p => p.SaleId == saleId).ToListAsync();
        Assert.All(plans, p => Assert.Equal(InstallmentStatus.Cancelled, p.Status));
    }

    [Fact]
    public async Task CannotReturnMoreThanWasSold_EvenAcrossTwoReturns()
    {
        using var db = TestDb.Create();
        var (saleId, saleItemId, _) = await SellAsync(db, qty: 3, paid: 3000m);
        var handler = new CreateSaleReturnCommandHandler(db);

        await handler.Handle(Return(saleId, saleItemId, 2), CancellationToken.None);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(Return(saleId, saleItemId, 2), CancellationToken.None)); // only 1 left
    }
}