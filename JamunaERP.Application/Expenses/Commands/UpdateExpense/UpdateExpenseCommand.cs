using FluentValidation;
using JamunaERP.Application.Common;
using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Expenses.Commands.UpdateExpense;

public record UpdateExpenseCommand(
    int Id,
    string Title,
    string Category,
    decimal Amount,
    DateTime ExpenseDate,
    string? Notes
) : IRequest<bool>;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExpenseDate)
            .Must(d => d.Date <= ShopTime.Today)
            .WithMessage("Expense date cannot be in the future.");
        RuleFor(x => x.Notes).MaximumLength(300);
    }
}

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateExpenseCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateExpenseCommand request, CancellationToken ct)
    {
        var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        expense.Title = request.Title.Trim();
        expense.Category = request.Category.Trim();
        expense.Amount = request.Amount;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Notes = request.Notes?.Trim();

        await _context.SaveChangesAsync(ct);
        return true;
    }
}