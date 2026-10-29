using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.PurchaseOrders.Commands.ReceivePurchaseOrder;
using JamunaERP.Application.Sales.Commands.CreateSale;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Entities;
using JamunaERP.Domain.Enums;
using JamunaERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.PurchaseOrders;

public class ReceivePurchaseOrderCommandHandlerTests
{
    private static async Task<PurchaseOrder> AddOrderAsync(
        AppDbContext db, params (int ProductId, int Arrived, int Faulty)[] items)
    {
        var dealer = new Dealer { Name = "Test Dealer", ContactPerson = "Mr X", Phone = "01800000000", Address = "Dhaka" };
        db.Dealers.Add(dealer);
        await db.SaveChangesAsync();

        var order = new PurchaseOrder
        {
            DealerInvoiceNo = "DINV-" + Guid.NewGuid().ToString("N")[..8],
            DealerId = dealer.Id,
            ArrivalDate = DateTime.UtcNow.Date
        };

        foreach (var item in items)
        {
            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                QuantityArrived = item.Arrived,
                FaultyQuantityReceived = item.Faulty
            });
        }

        db.PurchaseOrders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    [Fact]
    public async Task Receive_AddsGoodAndFaultyStock_AndMarksOrderReceived()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var order = await AddOrderAsync(db, (product.Id, 10, 2));
        var handler = new ReceivePurchaseOrderCommandHandler(db);

        var result = await handler.Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None);

        Assert.True(result);

        var good = await db.StockItems.AsNoTracking().SingleAsync(s => !s.IsFaulty);
        Assert.Equal(8, good.Quantity);
        Assert.Equal(StockLocationType.GoDown, good.Location);

        var faulty = await db.StockItems.AsNoTracking().SingleAsync(s => s.IsFaulty);
        Assert.Equal(2, faulty.Quantity);

        var savedOrder = await db.PurchaseOrders.AsNoTracking().SingleAsync(p => p.Id == order.Id);
        Assert.True(savedOrder.IsReceived);
        Assert.NotNull(savedOrder.ReceivedAt);
    }

    [Fact]
    public async Task Receive_AddsToTheExistingStockRow_InsteadOfCreatingADuplicate()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        await TestDb.AddStockAsync(db, product.Id, 5, StockLocationType.GoDown);
        var order = await AddOrderAsync(db, (product.Id, 10, 0));
        var handler = new ReceivePurchaseOrderCommandHandler(db);

        await handler.Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None);

        var good = await db.StockItems.AsNoTracking().Where(s => !s.IsFaulty).ToListAsync();
        var row = Assert.Single(good);
        Assert.Equal(15, row.Quantity);
    }

    [Fact]
    public async Task Receive_Twice_Throws_AndDoesNotDoubleCountStock()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var order = await AddOrderAsync(db, (product.Id, 10, 2));
        var handler = new ReceivePurchaseOrderCommandHandler(db);
        await handler.Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None));

        var good = await db.StockItems.AsNoTracking().SingleAsync(s => !s.IsFaulty);
        Assert.Equal(8, good.Quantity);
    }

    [Fact]
    public async Task Receive_FaultyMoreThanArrived_Throws_AndAddsNoStock()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var order = await AddOrderAsync(db, (product.Id, 2, 5));
        var handler = new ReceivePurchaseOrderCommandHandler(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None));

        Assert.Empty(await db.StockItems.AsNoTracking().ToListAsync());
        var savedOrder = await db.PurchaseOrders.AsNoTracking().SingleAsync(p => p.Id == order.Id);
        Assert.False(savedOrder.IsReceived);
    }

    [Fact]
    public async Task Receive_OrderWithNoItems_Throws()
    {
        using var db = TestDb.Create();
        var order = await AddOrderAsync(db);
        var handler = new ReceivePurchaseOrderCommandHandler(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(new ReceivePurchaseOrderCommand(order.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Receive_UnknownOrder_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var handler = new ReceivePurchaseOrderCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ReceivePurchaseOrderCommand(999), CancellationToken.None));
    }

    // ---------- End to end: purchase receiving feeds the sale ----------

    [Fact]
    public async Task ReceivedStock_CanBeSold_ButFaultyStockCannot()
    {
        using var db = TestDb.Create();
        var customer = await TestDb.AddCustomerAsync(db);
        var product = await TestDb.AddProductAsync(db, price: 1000m);
        var order = await AddOrderAsync(db, (product.Id, 10, 2)); // 8 good, 2 faulty
        await new ReceivePurchaseOrderCommandHandler(db)
            .Handle(new ReceivePurchaseOrderCommand(order.Id, StockLocationType.GoDown), CancellationToken.None);

        var saleHandler = new CreateSaleCommandHandler(db);

        // Selling 5 of the 8 good units works
        await saleHandler.Handle(
            new CreateSaleCommand(customer.Id, StockLocationType.GoDown, 5000m, false, 0, null,
                new List<CreateSaleItemInput> { new(product.Id, 5) }),
            CancellationToken.None);

        var good = await db.StockItems.AsNoTracking().SingleAsync(s => !s.IsFaulty);
        var faulty = await db.StockItems.AsNoTracking().SingleAsync(s => s.IsFaulty);
        Assert.Equal(3, good.Quantity);
        Assert.Equal(2, faulty.Quantity);

        // Only 3 good units remain, so selling 4 fails even though 2 faulty units exist
        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            saleHandler.Handle(
                new CreateSaleCommand(customer.Id, StockLocationType.GoDown, 4000m, false, 0, null,
                    new List<CreateSaleItemInput> { new(product.Id, 4) }),
                CancellationToken.None));
    }
}