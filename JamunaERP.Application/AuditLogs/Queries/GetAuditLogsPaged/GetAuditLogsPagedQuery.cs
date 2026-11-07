using JamunaERP.Application.AuditLogs.DTOs;
using JamunaERP.Application.Common.Interfaces;
using JamunaERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JamunaERP.Application.AuditLogs.Queries.GetAuditLogsPaged;

public record GetAuditLogsPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? EntityName = null,
    string? EntityId = null,
    string? Action = null,
    string? UserId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<PagedResult<AuditLogDto>>;

public class GetAuditLogsPagedQueryHandler
    : IRequestHandler<GetAuditLogsPagedQuery, PagedResult<AuditLogDto>>
{
    private readonly IAppDbContext _context;

    public GetAuditLogsPagedQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsPagedQuery request,
        CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName.Trim());

        if (!string.IsNullOrWhiteSpace(request.EntityId))
            query = query.Where(a => a.EntityId == request.EntityId.Trim());

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(a => a.Action == request.Action.Trim());

        if (!string.IsNullOrWhiteSpace(request.UserId))
            query = query.Where(a => a.UserId == request.UserId.Trim());

        if (request.From.HasValue)
            query = query.Where(a => a.TimestampUtc >= request.From.Value.Date);

        if (request.To.HasValue)
        {
            var endExclusive = request.To.Value.Date.AddDays(1);
            query = query.Where(a => a.TimestampUtc < endExclusive);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.TimestampUtc)
            .ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action,
                UserId = a.UserId,
                TimestampUtc = a.TimestampUtc,
                OldValues = a.OldValues,
                NewValues = a.NewValues
            })
            .ToListAsync(ct);

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}