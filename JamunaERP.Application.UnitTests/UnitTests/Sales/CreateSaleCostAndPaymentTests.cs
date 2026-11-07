using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.Sales;

public class CreateSaleCostAndPaymentTests
{
    [Fact]
    public async Task CashSale_SnapshotsCostPrice_AndRecordsReceipt()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m); // BuyingPrice = 800
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        var command = new CreateSaleCommand(
            customer.Id, StockLocationType.Showroom1, 3000m, false, 0, null,
            new List<CreateSaleItemInput> { new(product.Id, 3) },
            "MobileBanking");

        var saleId = await handler.Handle(command, CancellationToken.None);

        var item = await db.SaleItems.AsNoTracking().SingleAsync(i => i.SaleId == saleId);
        Assert.Equal(800m, item.CostPrice);

        var payment = await db.Payments.AsNoTracking().SingleAsync();
        Assert.Equal(saleId, payment.SaleId);
        Assert.Equal(3000m, payment.Amount);
        Assert.Equal("MobileBanking", payment.Method);
        Assert.StartsWith("RCP-", payment.ReceiptNo);
    }

    [Fact]
    public async Task CostPrice_StaysFixed_WhenProductCostChangesLater()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m); // BuyingPrice = 800
        await TestDb.AddStockAsync(db, product.Id, 10);
        var handler = new CreateSaleCommandHandler(db);

        var saleId = await handler.Handle(
            new CreateSaleCommand(customer.Id, StockLocationType.Showroom1, 1000m, false, 0, null,
                new List<CreateSaleItemInput> { new(product.Id, 1) }),
            CancellationToken.None);

        var tracked = await db.Products.SingleAsync(p => p.Id == product.Id);
        tracked.BuyingPrice = 900m;
        await db.SaveChangesAsync();

        var item = await db.SaleItems.AsNoTracking().SingleAsync(i => i.SaleId == saleId);
        Assert.Equal(800m, item.CostPrice);
    }
}