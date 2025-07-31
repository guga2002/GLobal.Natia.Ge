using Common.Domain.Contexts;
using Common.Domain.Interface;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Common.Domain.Services;

public class BackupService : IBackupService
{
    private readonly HttpClient _httpClient;
    private readonly IRedisService _redis;
    private readonly ILogger<BackupService> _logger;
    private static int _count = 0;

    public BackupService(HttpClient httpClient, IRedisService redis, ILogger<BackupService> logger)
    {
        _httpClient = httpClient;
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> Start()
    {
        _logger.LogInformation("Starting EMR backup process...");

        string emrWithProblems = string.Empty;
        var now = DateTime.Now;

        string basePath = @"B:\EMRS_Backups";
        Directory.CreateDirectory(basePath);
        var regionDir = Directory.CreateDirectory(Path.Combine(basePath, "Regions"));
        var stationDir = Directory.CreateDirectory(Path.Combine(basePath, "Station"));

        RegionStorage con = new RegionStorage();

        var backupTasks = con.Keys.Select(async item =>
        {
            string ip = $"192.168.{item}";
            string link = $"http://{ip}/goform/downldForm?";
            string subDirPath = (item.Contains("14.") || item.Contains("15.") || item.Contains("25.") || item.Contains("13."))
                ? Path.Combine(regionDir.FullName, item)
                : Path.Combine(stationDir.FullName, item);

            Directory.CreateDirectory(subDirPath);

            try
            {
                _logger.LogInformation("Requesting backup from {Ip}", ip);

                HttpResponseMessage response = await _httpClient.GetAsync(link);

                if (response.IsSuccessStatusCode)
                {
                    string fileName = Path.Combine(subDirPath, $"{now:yyyy_MM_dd_HH}(Hour).dat");

                    using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
                    await response.Content.CopyToAsync(fs);

                    _logger.LogInformation("Backup saved successfully: {FileName}", fileName);
                }
                else
                {
                    emrWithProblems += $"{ip};";
                    _logger.LogWarning("Failed to download from {Ip} - HTTP {StatusCode}", ip, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                emrWithProblems += $"{ip};";
                _logger.LogError(ex, "Exception occurred during backup from {Ip}", ip);
            }
        });

        await Task.WhenAll(backupTasks);

        if (!string.IsNullOrEmpty(emrWithProblems))
        {
            await _redis.SetAsync("EMRWHICHHAVEPROBLEMWHILEBACKUP", emrWithProblems, TimeSpan.FromSeconds(10));
            _logger.LogWarning("Backup completed with some EMRs failing: {List}", emrWithProblems);
        }

        string monthName = now.ToString("MMMM", new CultureInfo("ka-GE"));
        string backupMessage = now.Day == 1
            ? $"პირველი {monthName}-ის, სისტემის ბექაფი გადმოვწერე და შევინახე სერვერზე, როგორც რეგიონის ასევე სადგურის."
            : $"{now.Day}, {monthName}-ის, სისტემის ბექაფი გადმოვწერე და შევინახე სერვერზე, როგორც რეგიონის ასევე სადგურის.";

        await _redis.SetAsync("BACKUPTAKECOMPLETESUCCESFULLY", backupMessage, TimeSpan.FromSeconds(30));

        _logger.LogInformation("Backup process finished successfully.");

        return true;
    }

    private DateTime? _lastBackupTime = null;

    public bool IsTimeForBackup()
    {
        var now = DateTime.Now;
        var backupHours = new[] { 11, 23 };

        if (backupHours.Contains(now.Hour) && now.Minute <= 10)
        {
            if (_lastBackupTime == null || _lastBackupTime.Value.Hour != now.Hour)
            {
                _lastBackupTime = now;
                _logger.LogInformation("Backup triggered at {Time}", now);
                return true;
            }
        }

        return false;
    }
}
