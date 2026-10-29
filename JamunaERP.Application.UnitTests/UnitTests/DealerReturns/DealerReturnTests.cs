using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.DealerPayments.Queries.GetDealerBalance;
using JamunaERP.Application.DealerReturns.Commands.CreateDealerReturn;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.DealerReturns;

public class DealerReturnTests
{
    [Fact]
    public async Task Return_RemovesFaultyStock_AndReducesDealerBalance()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 5, StockLocationType.GoDown, isFaulty: true);

        var dealer = new Dealer { Name = "D1" };
        var order = new PurchaseOrder { DealerInvoiceNo = "INV-1", Dealer = dealer, ArrivalDate = DateTime.UtcNow };
        order.Items.Add(new PurchaseOrderItem { ProductId = product.Id, QuantityArrived = 10, UnitCost = 100m });
        db.PurchaseOrders.Add(order);
        db.DealerPayments.Add(new DealerPayment { Dealer = dealer, Amount = 300m });
        await db.SaveChangesAsync();

        await new CreateDealerReturnCommandHandler(db).Handle(
            new CreateDealerReturnCommand(dealer.Id, product.Id, StockLocationType.GoDown, 2, 200m, "Faulty motor", null),
            CancellationToken.None);

        Assert.Equal(3, await db.StockItems.Where(s => s.IsFaulty).SumAsync(s => s.Quantity));

        var balance = await new GetDealerBalanceQueryHandler(db)
            .Handle(new GetDealerBalanceQuery(dealer.Id), CancellationToken.None);
        Assert.Equal(1000m, balance.TotalPurchased);
        Assert.Equal(200m, balance.TotalReturned);
        Assert.Equal(500m, balance.Outstanding);   // 1000 - 200 - 300
    }

    [Fact]
    public async Task Return_MoreThanFaultyStock_Throws()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var dealer = new Dealer { Name = "D1" };
        db.Dealers.Add(dealer);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InsufficientStockException>(() => new CreateDealerReturnCommandHandler(db).Handle(
            new CreateDealerReturnCommand(dealer.Id, product.Id, StockLocationType.GoDown, 1, null, "Faulty", null),
            CancellationToken.None));
    }

    [Fact]
    public async Task Return_DefaultCredit_IsQuantityTimesBuyingPrice()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db, price: 1000m); // BuyingPrice = 800
        await TestDb.AddStockAsync(db, product.Id, 3, StockLocationType.GoDown, isFaulty: true);
        var dealer = new Dealer { Name = "D1" };
        db.Dealers.Add(dealer);
        await db.SaveChangesAsync();

        var id = await new CreateDealerReturnCommandHandler(db).Handle(
            new CreateDealerReturnCommand(dealer.Id, product.Id, StockLocationType.GoDown, 2, null, "Faulty", null),
            CancellationToken.None);

        var saved = await db.DealerReturns.AsNoTracking().SingleAsync(r => r.Id == id);
        Assert.Equal(1600m, saved.CreditAmount);
    }
}