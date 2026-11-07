using JamunaERP.Application.InstallmentPlans.Commands.PayInstallment;
using JamunaERP.Application.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.InstallmentPlans;

public class PayInstallmentReceiptTests
{
    [Fact]
    public async Task Payment_CreatesReceiptLinkedToSale()
    {
        using var db = TestDb.Create();
        var (sale, plans) = await TestDb.AddInstallmentSaleAsync(db); // total 1000, paid 100, 3 x 300
        var handler = new PayInstallmentCommandHandler(db);

        await handler.Handle(
            new PayInstallmentCommand(plans[0].Id, 300m, "Card"),
            CancellationToken.None);

        var payment = await db.Payments.AsNoTracking().SingleAsync();
        Assert.Equal(sale.Id, payment.SaleId);
        Assert.Equal(300m, payment.Amount);
        Assert.Equal("Card", payment.Method);
        Assert.StartsWith("RCP-", payment.ReceiptNo);
    }
}