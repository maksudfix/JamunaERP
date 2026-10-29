using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;

namespace JamunaERP.Application.Expenses.Commands.CreateExpense;

public record CreateExpenseCommand(
    string Title,
    string Category,
    decimal Amount,
    DateTime? ExpenseDate,
    string? Notes
) : IRequest<int>;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExpenseDate)
            .Must(d => d is null || d.Value.Date <= ShopTime.Today)
            .WithMessage("Expense date cannot be in the future.");
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, int>
{
    private readonly IAppDbContext _context;

    public CreateExpenseCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        var expense = new Expense
        {
            Title = request.Title.Trim(),
            Category = request.Category.Trim(),
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate ?? ShopTime.ToLocal(DateTime.UtcNow),
            Notes = request.Notes?.Trim()
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(ct);
        return expense.Id;
    }
}