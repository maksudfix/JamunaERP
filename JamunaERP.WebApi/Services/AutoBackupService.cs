using JamunaERP.Application.Contracts;

namespace JamunaERP.WebApi.Services;

// Runs a backup whenever the newest one is older than Backup:EveryHours.
// Checking hourly means a missed night (PC switched off) is caught up as soon as the app starts.
public class AutoBackupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<AutoBackupService> _logger;

    public AutoBackupService(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<AutoBackupService> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.GetValue("Backup:AutoEnabled", true))
        {
            _logger.LogInformation("Automatic backups are disabled.");
            return;
        }

        var maxAge = TimeSpan.FromHours(Math.Max(1, _config.GetValue("Backup:EveryHours", 24)));

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
            do
            {
                await RunIfDueAsync(maxAge, stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // app is shutting down
        }
    }

    private async Task RunIfDueAsync(TimeSpan maxAge, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var backups = scope.ServiceProvider.GetRequiredService<IBackupService>();

            var latest = (await backups.ListBackupsAsync(ct)).FirstOrDefault();
            if (latest is null || DateTime.UtcNow - latest.CreatedAtUtc >= maxAge)
            {
                var info = await backups.CreateBackupAsync(ct);
                _logger.LogInformation("Automatic backup finished: {File}", info.FileName);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automatic backup failed");
        }
    }
}