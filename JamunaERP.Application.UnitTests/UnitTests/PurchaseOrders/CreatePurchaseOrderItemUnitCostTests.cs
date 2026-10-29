using JamunaERP.Application.PurchaseOrderItems.Commands.CreatePurchaseOrderItem;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.PurchaseOrders;

public class CreatePurchaseOrderItemUnitCostTests
{
    [Fact]
    public async Task Create_SavesUnitCost()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var order = new PurchaseOrder
        {
            DealerInvoiceNo = "INV-1",
            Dealer = new Dealer { Name = "D1" },
            ArrivalDate = DateTime.UtcNow
        };
        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();

        var handler = new CreatePurchaseOrderItemCommandHandler(db);
        var id = await handler.Handle(
            new CreatePurchaseOrderItemCommand(order.Id, product.Id, 10, 1, 750m),
            CancellationToken.None);

        var item = await db.PurchaseOrderItems.AsNoTracking().SingleAsync(i => i.Id == id);
        Assert.Equal(750m, item.UnitCost);
    }

    [Fact]
    public void Validator_RejectsZeroUnitCost()
    {
        var validator = new CreatePurchaseOrderItemCommandValidator();

        var result = validator.Validate(new CreatePurchaseOrderItemCommand(1, 1, 5, 0, 0m));

        Assert.False(result.IsValid);
    }
}