using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Expenses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Expenses.Queries.GetExpenses;

public record GetExpensesQuery(DateTime? From, DateTime? To, string? Category) : IRequest<List<ExpenseDto>>;

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, List<ExpenseDto>>
{
    private readonly IAppDbContext _context;

    public GetExpensesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        var query = _context.Expenses.AsNoTracking().AsQueryable();

        if (request.From.HasValue)
            query = query.Where(e => e.ExpenseDate >= request.From.Value.Date);

        if (request.To.HasValue)
        {
            var endExclusive = request.To.Value.Date.AddDays(1);
            query = query.Where(e => e.ExpenseDate < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();
            query = query.Where(e => e.Category == category);
        }

        return await query
            .OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.Id)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Title = e.Title,
                Category = e.Category,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                Notes = e.Notes
            })
            .ToListAsync(ct);
    }
}