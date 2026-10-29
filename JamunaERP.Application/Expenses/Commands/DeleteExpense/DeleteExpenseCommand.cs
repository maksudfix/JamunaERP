using JamunaERP.Application.Common.Exceptions;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Expenses.Commands.DeleteExpense;

public record DeleteExpenseCommand(int Id) : IRequest<bool>;

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteExpenseCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteExpenseCommand request, CancellationToken ct)
    {
        var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Expense), request.Id);

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}