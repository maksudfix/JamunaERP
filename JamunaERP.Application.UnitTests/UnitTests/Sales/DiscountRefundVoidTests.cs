using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.SaleReturns.Commands.CreateSaleReturn;
using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.Sales.Commands.VoidSale;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using JamunaERP.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JamunaERP.Application.UnitTests.Sales;

public class DiscountRefundVoidTests
{
    private static async Task<int> SellAsync(AppDbContext db, int qty, decimal paid, decimal discount = 0)
    {
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        await TestDb.AddStockAsync(db, product.Id, 10);

        return await new CreateSaleCommandHandler(db).Handle(
            new CreateSaleCommand(customer.Id, StockLocationType.Showroom1, paid, false, 0, null,
                new List<CreateSaleItemInput> { new(product.Id, qty) }, "Cash", discount),
            CancellationToken.None);
    }

    private static async Task<int> FirstItemIdAsync(AppDbContext db, int saleId) =>
        (await db.SaleItems.AsNoTracking().FirstAsync(i => i.SaleId == saleId)).Id;

    private static VoidSaleCommandHandler NewVoidHandler(AppDbContext db)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAppDbContext>(db);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(VoidSaleCommand).Assembly));
        var sender = services.BuildServiceProvider().GetRequiredService<ISender>();
        return new VoidSaleCommandHandler(db, sender);
    }

    [Fact]
    public async Task Void_OnDiscountedSale_NeverRefundsMoreThanPaid()
    {
        using var db = TestDb.Create();
        // 3 x 1000 = 3000 gross, discount 0.01 -> factor creates rounding drift per line
        var saleId = await SellAsync(db, qty: 3, paid: 2999.99m, discount: 0.01m);

        var result = await NewVoidHandler(db).Handle(
            new VoidSaleCommand(saleId, "Test", StockLocationType.Showroom1), CancellationToken.None);

        var sale = await db.Sales.AsNoTracking().SingleAsync(s => s.Id == saleId);
        Assert.Equal(sale.TotalAmount, sale.ReturnedAmount);
        Assert.Equal(sale.PaidAmount, sale.RefundedAmount);
        Assert.Equal(sale.PaidAmount, result.RefundAmount);
    }

    [Fact]
    public async Task Discount_ReducesTotal_AndIsStored()
    {
        using var db = TestDb.Create();

        var saleId = await SellAsync(db, qty: 3, paid: 2500m, discount: 500m);

        var sale = await db.Sales.AsNoTracking().SingleAsync(s => s.Id == saleId);
        Assert.Equal(2500m, sale.TotalAmount);
        Assert.Equal(500m, sale.DiscountAmount);
    }

    [Fact]
    public async Task Discount_AboveSaleAmount_Throws()
    {
        using var db = TestDb.Create();

        await Assert.ThrowsAsync<BusinessRuleException>(() => SellAsync(db, qty: 1, paid: 0m, discount: 1500m));
    }

    [Fact]
    public async Task Return_OnDiscountedSale_IsProRated()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db, qty: 2, paid: 1800m, discount: 200m); // gross 2000, net 1800
        var itemId = await FirstItemIdAsync(db, saleId);

        var result = await new CreateSaleReturnCommandHandler(db).Handle(
            new CreateSaleReturnCommand(saleId, "Changed mind", StockLocationType.Showroom1,
                new List<SaleReturnLineInput> { new(itemId, 1) }),
            CancellationToken.None);

        Assert.Equal(900m, result.ReturnedValue);
        Assert.Equal(900m, result.RefundAmount);
    }

    [Fact]
    public async Task Refund_IsRecordedAsNegativePayment()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db, qty: 3, paid: 3000m);
        var itemId = await FirstItemIdAsync(db, saleId);

        await new CreateSaleReturnCommandHandler(db).Handle(
            new CreateSaleReturnCommand(saleId, "Changed mind", StockLocationType.Showroom1,
                new List<SaleReturnLineInput> { new(itemId, 1) }),
            CancellationToken.None);

        var refund = await db.Payments.AsNoTracking().SingleAsync(p => p.Method == "Refund");
        Assert.Equal(-1000m, refund.Amount);
        Assert.Equal(2000m, await db.Payments.SumAsync(p => p.Amount)); // 3000 in, 1000 out
    }

    [Fact]
    public async Task Void_ReturnsEverythingNotYetReturned()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db, qty: 3, paid: 3000m);
        var itemId = await FirstItemIdAsync(db, saleId);

        await new CreateSaleReturnCommandHandler(db).Handle(
            new CreateSaleReturnCommand(saleId, "Partial", StockLocationType.Showroom1,
                new List<SaleReturnLineInput> { new(itemId, 1) }),
            CancellationToken.None);

        var result = await NewVoidHandler(db).Handle(
            new VoidSaleCommand(saleId, "Wrong customer", StockLocationType.Showroom1),
            CancellationToken.None);

        Assert.Equal(2000m, result.ReturnedValue);

        var sale = await db.Sales.AsNoTracking().SingleAsync(s => s.Id == saleId);
        Assert.Equal(3000m, sale.ReturnedAmount);
        Assert.Equal(3000m, sale.RefundedAmount);
        Assert.Equal(10, await db.StockItems.Where(s => !s.IsFaulty).SumAsync(s => s.Quantity));
    }

    [Fact]
    public async Task Void_TwiceThrows()
    {
        using var db = TestDb.Create();
        var saleId = await SellAsync(db, qty: 1, paid: 1000m);
        var handler = NewVoidHandler(db);
        var command = new VoidSaleCommand(saleId, "Test", StockLocationType.Showroom1);

        await handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(command, CancellationToken.None));
    }
}