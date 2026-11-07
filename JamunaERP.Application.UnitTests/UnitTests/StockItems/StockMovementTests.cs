using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.StockAdjustments.Commands.AdjustStock;
using JamunaERP.Application.StockItems.Commands.MoveStock;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.StockItems;

public class StockMovementTests
{
    [Fact]
    public async Task Move_PartialQuantity_SplitsStockBetweenLocations()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 10, StockLocationType.GoDown);

        await new MoveStockCommandHandler(db).Handle(
            new MoveStockCommand(product.Id, StockLocationType.GoDown, StockLocationType.Showroom1, 4),
            CancellationToken.None);

        var godown = await db.StockItems.Where(s => s.Location == StockLocationType.GoDown).SumAsync(s => s.Quantity);
        var showroom = await db.StockItems.Where(s => s.Location == StockLocationType.Showroom1).SumAsync(s => s.Quantity);
        Assert.Equal(6, godown);
        Assert.Equal(4, showroom);
    }

    [Fact]
    public async Task Move_IntoLocationThatAlreadyHasStock_MergesIntoOneRow()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 5, StockLocationType.GoDown);
        await TestDb.AddStockAsync(db, product.Id, 2, StockLocationType.Showroom1);

        await new MoveStockCommandHandler(db).Handle(
            new MoveStockCommand(product.Id, StockLocationType.GoDown, StockLocationType.Showroom1, 5),
            CancellationToken.None);

        var rows = await db.StockItems.AsNoTracking()
            .Where(s => s.Location == StockLocationType.Showroom1 && !s.IsFaulty && s.SerialNumber == null)
            .ToListAsync();

        Assert.Equal(7, Assert.Single(rows).Quantity);
    }

    [Fact]
    public async Task Move_MoreThanAvailable_Throws()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 3, StockLocationType.GoDown);

        await Assert.ThrowsAsync<InsufficientStockException>(() => new MoveStockCommandHandler(db).Handle(
            new MoveStockCommand(product.Id, StockLocationType.GoDown, StockLocationType.Showroom1, 5),
            CancellationToken.None));
    }

    [Fact]
    public async Task Adjust_Negative_ReducesStock_AndIsLogged()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 10);

        await new AdjustStockCommandHandler(db).Handle(
            new AdjustStockCommand(product.Id, StockLocationType.Showroom1, false, -2, "Damaged in shop"),
            CancellationToken.None);

        Assert.Equal(8, await db.StockItems.SumAsync(s => s.Quantity));
        var log = await db.StockAdjustments.AsNoTracking().SingleAsync();
        Assert.Equal(-2, log.QuantityChange);
    }

    [Fact]
    public async Task Adjust_Positive_CreatesRowWhenNoneExists()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);

        await new AdjustStockCommandHandler(db).Handle(
            new AdjustStockCommand(product.Id, StockLocationType.GoDown, false, 5, "Found during count"),
            CancellationToken.None);

        Assert.Equal(5, await db.StockItems.SumAsync(s => s.Quantity));
    }

    [Fact]
    public async Task Adjust_BelowZero_Throws()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 1);

        await Assert.ThrowsAsync<InsufficientStockException>(() => new AdjustStockCommandHandler(db).Handle(
            new AdjustStockCommand(product.Id, StockLocationType.Showroom1, false, -5, "Lost"),
            CancellationToken.None));
    }
}