using JamunaERP.Application.PurchaseOrders.Commands.ReceivePurchaseOrder;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.PurchaseOrders;

public class ReceiveUpdatesCostTests
{
    [Fact]
    public async Task Receive_SetsProductBuyingPrice_ToLatestUnitCost()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db, price: 1000m); // BuyingPrice = 800
        var order = new PurchaseOrder
        {
            DealerInvoiceNo = "INV-1",
            Dealer = new Dealer { Name = "D1" },
            ArrivalDate = DateTime.UtcNow
        };
        order.Items.Add(new PurchaseOrderItem { ProductId = product.Id, QuantityArrived = 5, UnitCost = 900m });
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();

        await new ReceivePurchaseOrderCommandHandler(db)
            .Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None);

        var saved = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
        Assert.Equal(900m, saved.BuyingPrice);
    }
}