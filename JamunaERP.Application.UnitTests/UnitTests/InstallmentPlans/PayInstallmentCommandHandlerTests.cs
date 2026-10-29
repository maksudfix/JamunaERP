using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.InstallmentPlans.Commands.PayInstallment;
using JamunaERP.Application.UnitTests.Helpers;
using JamunaERP.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.UnitTests.InstallmentPlans;

public class PayInstallmentCommandHandlerTests
{
    private static PayInstallmentCommand Pay(int planId, decimal amount) => new(planId, amount);

    [Fact]
    public async Task FullPayment_MarksInstallmentPaid_AndUpdatesSalePaidAmount()
    {
        using var db = TestDb.Create();
        var (sale, plans) = await TestDb.AddInstallmentSaleAsync(db); // total 1000, paid 100, 3 x 300
        var handler = new PayInstallmentCommandHandler(db);

        await handler.Handle(Pay(plans[0].Id, 300m), CancellationToken.None);

        var plan = await db.InstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == plans[0].Id);
        Assert.Equal(InstallmentStatus.Paid, plan.Status);
        Assert.Equal(300m, plan.AmountPaid);
        Assert.NotNull(plan.PaymentDate);

        var savedSale = await db.Sales.AsNoTracking().SingleAsync(s => s.Id == sale.Id);
        Assert.Equal(400m, savedSale.PaidAmount); // 100 down payment + 300
    }

    [Fact]
    public async Task PartialPayment_KeepsInstallmentPending_UntilFullyPaid()
    {
        using var db = TestDb.Create();
        var (sale, plans) = await TestDb.AddInstallmentSaleAsync(db);
        var handler = new PayInstallmentCommandHandler(db);

        await handler.Handle(Pay(plans[0].Id, 100m), CancellationToken.None);

        var afterFirst = await db.InstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == plans[0].Id);
        Assert.Equal(InstallmentStatus.Pending, afterFirst.Status);
        Assert.Equal(100m, afterFirst.AmountPaid);

        await handler.Handle(Pay(plans[0].Id, 200m), CancellationToken.None);

        var afterSecond = await db.InstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == plans[0].Id);
        Assert.Equal(InstallmentStatus.Paid, afterSecond.Status);
        Assert.Equal(300m, afterSecond.AmountPaid);

        var savedSale = await db.Sales.AsNoTracking().SingleAsync(s => s.Id == sale.Id);
        Assert.Equal(400m, savedSale.PaidAmount);
    }

    [Fact]
    public async Task PartialPayment_OnPastDueInstallment_IsOverdue()
    {
        using var db = TestDb.Create();
        var (_, plans) = await TestDb.AddInstallmentSaleAsync(db, firstDue: DateTime.UtcNow.Date.AddDays(-5));
        var handler = new PayInstallmentCommandHandler(db);

        await handler.Handle(Pay(plans[0].Id, 100m), CancellationToken.None);

        var plan = await db.InstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == plans[0].Id);
        Assert.Equal(InstallmentStatus.Overdue, plan.Status);
        Assert.Equal(100m, plan.AmountPaid);
    }

    [Fact]
    public async Task Overpayment_Throws_AndChangesNothing()
    {
        using var db = TestDb.Create();
        var (sale, plans) = await TestDb.AddInstallmentSaleAsync(db);
        var handler = new PayInstallmentCommandHandler(db);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(Pay(plans[0].Id, 301m), CancellationToken.None));

        var plan = await db.InstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == plans[0].Id);
        Assert.Equal(0m, plan.AmountPaid);
        Assert.Equal(InstallmentStatus.Pending, plan.Status);

        var savedSale = await db.Sales.AsNoTracking().SingleAsync(s => s.Id == sale.Id);
        Assert.Equal(100m, savedSale.PaidAmount);
    }

    [Fact]
    public async Task PayingAnAlreadyPaidInstallment_Throws()
    {
        using var db = TestDb.Create();
        var (_, plans) = await TestDb.AddInstallmentSaleAsync(db);
        var handler = new PayInstallmentCommandHandler(db);
        await handler.Handle(Pay(plans[0].Id, 300m), CancellationToken.None);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.Handle(Pay(plans[0].Id, 1m), CancellationToken.None));

        Assert.Contains("already", ex.Message);
    }

    [Fact]
    public async Task UnknownInstallment_ThrowsNotFound()
    {
        using var db = TestDb.Create();
        var handler = new PayInstallmentCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(Pay(999, 100m), CancellationToken.None));
    }
}