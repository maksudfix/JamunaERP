using JamunaERP.Application.StockItems.Commands.CreateStockItem;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.StockItems;

public class StockItemCommandHandlerTests
{
    [Fact]
    public async Task CreateStockItem_PersistsTheRow()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var handler = new CreateStockItemCommandHandler(db);

        var id = await handler.Handle(
            new CreateStockItemCommand(product.Id, StockLocationType.GoDown, 4, false, "SN-001"),
            CancellationToken.None);

        var stock = await db.StockItems.AsNoTracking().SingleAsync(s => s.Id == id);
        Assert.Equal(product.Id, stock.ProductId);
        Assert.Equal(StockLocationType.GoDown, stock.Location);
        Assert.Equal(4, stock.Quantity);
        Assert.False(stock.IsFaulty);
        Assert.Equal("SN-001", stock.SerialNumber);
    }
}