using JamunaERP.Application.Common;
using JamunaERP.Application.Expenses.Commands.CreateExpense;
using JamunaERP.Application.Expenses.Queries.GetExpenses;
using JamunaERP.Application.UnitTests.Helpers;

namespace JamunaERP.Application.UnitTests.Expenses;

public class ExpenseTests
{
    [Fact]
    public async Task Create_WithoutDate_UsesShopLocalTime()
    {
        using var db = TestDb.Create();
        var id = await new CreateExpenseCommandHandler(db).Handle(
            new CreateExpenseCommand("Tea", "Misc", 50m, null, null), CancellationToken.None);
        var saved = await db.Expenses.FindAsync(id);
        Assert.Equal(ShopTime.Today, saved!.ExpenseDate.Date);
    }

    [Fact]
    public async Task Create_ThenFilterByCategoryAndDate()
    {
        using var db = TestDb.Create();
        var create = new CreateExpenseCommandHandler(db);

        await create.Handle(new CreateExpenseCommand("Shop rent", "Rent", 15000m, new DateTime(2026, 10, 5), null), CancellationToken.None);
        await create.Handle(new CreateExpenseCommand("Electricity", "Utilities", 3000m, new DateTime(2026, 10, 10), null), CancellationToken.None);
        await create.Handle(new CreateExpenseCommand("Old rent", "Rent", 14000m, new DateTime(2026, 9, 5), null), CancellationToken.None);

        var query = new GetExpensesQueryHandler(db);

        var rent = await query.Handle(new GetExpensesQuery(null, null, "Rent"), CancellationToken.None);
        Assert.Equal(2, rent.Count);

        var october = await query.Handle(
            new GetExpensesQuery(new DateTime(2026, 10, 1), new DateTime(2026, 10, 31), null), CancellationToken.None);
        Assert.Equal(2, october.Count);
        Assert.Equal(18000m, october.Sum(e => e.Amount));
    }

    [Fact]
    public void Validator_RejectsZeroAmount()
    {
        var result = new CreateExpenseCommandValidator()
            .Validate(new CreateExpenseCommand("Rent", "Rent", 0m, null, null));

        Assert.False(result.IsValid);
    }
}