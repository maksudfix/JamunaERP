namespace JamunaERP.Application.Contracts;

public record BackupInfo(string FileName, long SizeBytes, DateTime CreatedAtUtc);

public interface IBackupService
{
    Task<BackupInfo> CreateBackupAsync(CancellationToken ct);
    Task<IReadOnlyList<BackupInfo>> ListBackupsAsync(CancellationToken ct);
}