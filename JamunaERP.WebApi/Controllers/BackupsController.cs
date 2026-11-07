using JamunaERP.Application.Common.Security;
using JamunaERP.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JamunaERP.WebApi.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public class BackupsController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupsController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    // GET: api/backups
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BackupInfo>>> ListBackups(CancellationToken ct)
    {
        var backups = await _backupService.ListBackupsAsync(ct);
        return Ok(backups);
    }

    // POST: api/backups
    [HttpPost]
    public async Task<ActionResult<BackupInfo>> CreateBackup(CancellationToken ct)
    {
        var info = await _backupService.CreateBackupAsync(ct);
        return Ok(info);
    }
}