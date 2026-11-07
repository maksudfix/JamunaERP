using JamunaERP.Application.AuditLogs.DTOs;
using JamunaERP.Application.AuditLogs.Queries.GetAuditLogsPaged;
using JamunaERP.Application.Common.Models;
using JamunaERP.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public class AuditLogsController(ISender mediator) : ControllerBase
{
    // GET: api/auditlogs/paged?page=1&pageSize=20&entityName=Product&action=Modified
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetPaged(
        [FromQuery] GetAuditLogsPagedQuery query,
        CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }
}