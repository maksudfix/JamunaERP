using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Expenses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.Expenses.Queries.GetExpensesPaged;

public record GetExpensesPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    DateTime? From = null,
    DateTime? To = null,
    string? Category = null
) : IRequest<PagedResult<ExpenseDto>>;

public class GetExpensesPagedQueryHandler
    : IRequestHandler<GetExpensesPagedQuery, PagedResult<ExpenseDto>>
{
    private readonly IAppDbContext _context;

    public GetExpensesPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ExpenseDto>> Handle(
        GetExpensesPagedQuery request,
        CancellationToken ct)
    {
        var (page, pageSize) =
            Paging.Normalize(request.Page, request.PageSize);

        var query = _context.Expenses
            .AsNoTracking()
            .AsQueryable();

        // Inclusive start date.
        if (request.From.HasValue)
        {
            var startDate = request.From.Value.Date;

            query = query.Where(e =>
                e.ExpenseDate >= startDate);
        }

        // Inclusive end date, implemented using an exclusive boundary.
        if (request.To.HasValue)
        {
            var endExclusive = request.To.Value.Date.AddDays(1);

            query = query.Where(e =>
                e.ExpenseDate < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim();

            query = query.Where(e =>
                e.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();

            query = query.Where(e =>
                e.Title.Contains(term) ||
                (e.Notes != null && e.Notes.Contains(term)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResult<ExpenseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}