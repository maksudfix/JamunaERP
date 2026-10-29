using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.DealerPayments.Commands.RecordDealerPayment;
using JamunaERP.Application.DealerPayments.Queries.GetDealerBalance;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.DealerPayments;

public class DealerPaymentTests
{
    [Fact]
    public async Task Record_SavesPayment_WithDefaultDate()
    {
        using var db = TestDb.Create();
        var dealer = new Dealer { Name = "D1" };
        db.Dealers.Add(dealer);
        await db.SaveChangesAsync();

        var handler = new RecordDealerPaymentCommandHandler(db);
        var id = await handler.Handle(
            new RecordDealerPaymentCommand(dealer.Id, 500m, null, " first installment "),
            CancellationToken.None);

        var saved = await db.DealerPayments.AsNoTracking().SingleAsync(p => p.Id == id);
        Assert.Equal(500m, saved.Amount);
        Assert.Equal("first installment", saved.Notes);
        Assert.True(saved.Date > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Record_UnknownDealer_Throws()
    {
        using var db = TestDb.Create();
        var handler = new RecordDealerPaymentCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new RecordDealerPaymentCommand(999, 100m, null, null),
            CancellationToken.None));
    }

    [Fact]
    public void Validator_RejectsZeroAmount()
    {
        var result = new RecordDealerPaymentCommandValidator()
            .Validate(new RecordDealerPaymentCommand(1, 0m, null, null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Balance_IsPurchasedMinusPaid()
    {
        using var db = TestDb.Create();
        var product = await TestDb.AddProductAsync(db);
        var dealer = new Dealer { Name = "D1" };
        var order = new PurchaseOrder
        {
            DealerInvoiceNo = "INV-1",
            Dealer = dealer,
            ArrivalDate = DateTime.UtcNow
        };
        order.Items.Add(new PurchaseOrderItem { ProductId = product.Id, QuantityArrived = 10, UnitCost = 100m });
        db.PurchaseOrders.Add(order);
        db.DealerPayments.Add(new DealerPayment { Dealer = dealer, Amount = 300m });
        await db.SaveChangesAsync();

        var result = await new GetDealerBalanceQueryHandler(db)
            .Handle(new GetDealerBalanceQuery(dealer.Id), CancellationToken.None);

        Assert.Equal(1000m, result.TotalPurchased);
        Assert.Equal(300m, result.TotalPaid);
        Assert.Equal(700m, result.Outstanding);
    }
}