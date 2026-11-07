using JamunaERP.Application.Contracts;
using JamunaERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JamunaERP.Infrastructure.Services;

public class SqlBackupService : IBackupService
{
    private readonly AppDbContext _db;
    private readonly string _backupFolder;

    public SqlBackupService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        var configuredFolder = configuration["Backup:Folder"];
        _backupFolder = string.IsNullOrWhiteSpace(configuredFolder)
            ? Path.Combine(AppContext.BaseDirectory, "Backups")
            : configuredFolder;
    }

    public async Task<BackupInfo> CreateBackupAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_backupFolder);

        var dbName = _db.Database.GetDbConnection().Database;
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Database name could not be determined from the connection string.");

        var safeFilePrefix = string.Concat(dbName.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{safeFilePrefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
        var fullPath = Path.Combine(_backupFolder, fileName);

        var escapedDbName = dbName.Replace("]", "]]");

        // 1. Full database backup (includes Products, StockItems, Sales, Customers, Dealers, etc.)
        var backupSql = $"BACKUP DATABASE [{escapedDbName}] TO DISK = @p0 WITH INIT, CHECKSUM";
        await _db.Database.ExecuteSqlRawAsync(backupSql, new object[] { fullPath }, ct);

        // 2. Ask SQL Server to verify the backup file is readable and intact
        const string verifySql = "RESTORE VERIFYONLY FROM DISK = @p0 WITH CHECKSUM";
        await _db.Database.ExecuteSqlRawAsync(verifySql, new object[] { fullPath }, ct);

        var fileInfo = new FileInfo(fullPath);
        return new BackupInfo(fileInfo.Name, fileInfo.Length, fileInfo.CreationTimeUtc);
    }

    public Task<IReadOnlyList<BackupInfo>> ListBackupsAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_backupFolder))
            return Task.FromResult<IReadOnlyList<BackupInfo>>(Array.Empty<BackupInfo>());

        var backups = new DirectoryInfo(_backupFolder)
            .GetFiles("*.bak")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new BackupInfo(f.Name, f.Length, f.CreationTimeUtc))
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupInfo>>(backups);
    }
}