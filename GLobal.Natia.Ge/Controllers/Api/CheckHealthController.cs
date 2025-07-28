using Common.Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CheckHealthController : ControllerBase
{
    private readonly IRedisService _redis;
    private readonly ILogger<CheckHealthController> _logger;

    public CheckHealthController(IRedisService redis, ILogger<CheckHealthController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private static readonly object _printerLock = new();
    private static readonly object _systemLock = new();

    private static DateTime _lastPrinterCheck = DateTime.MinValue;
    private static DateTime _lastSystemCheck = DateTime.MinValue;

    private const string BackupKey = "BACKUPTAKECOMPLETESUCCESFULLY";
    private const string SystemHealthKey = "SystemHealth";
    private const string PrinterIp = "192.168.0.160";

    private static readonly string[] PrinterErrorMessages =
    {
        "პრინტერზე წვდომა დავკარგე, გთხოვ შეამოწმო და პრობლემა სწრაფად აღმოფხვრა.",
        "პრინტერი არ მუშაობს, იქნებ შეამოწმე და დროულად მოაგვარო ეს საკითხი?",
        "პრინტერთან კავშირი დავკარგე. გთხოვ სწრაფად გაიგე, რა მოხდა და გამოაწორე, მადლობა.",
        "პრინტერი მიუწვდომელია, იქნებ პრობლემას ყურადღება მიაქციო და დროულად მოაგვარო.",
        "პრინტერთან წვდომა დავკარგე. გთხოვ, გაიგე რა მოხდა და მოაგვარე ეს პრობლემა."
    };

    [HttpGet("health")]
    public async Task<IActionResult> GetSystemHealth()
    {
        var now = DateTime.Now;
        _logger.LogDebug("System health check triggered at {Time}", now);

        // Printer health check (every 5 mins)
        if (ShouldCheck(ref _lastPrinterCheck, now, minutes: 5, _printerLock))
        {
            _logger.LogInformation("Pinging printer at IP {Ip}...", PrinterIp);
            var pingResult = await PingPrinterAsync(PrinterIp);
            if (!pingResult)
            {
                var error = PrinterErrorMessages[Random.Shared.Next(PrinterErrorMessages.Length)];
                _logger.LogWarning("Printer is unreachable. Returning error message: {Message}", error);
                return Ok(error);
            }
            else
            {
                _logger.LogDebug("Printer ping successful.");
            }
        }

        // Backup health message
        var backupMessage = await _redis.GetAsync<string>(BackupKey);
        if (!string.IsNullOrEmpty(backupMessage))
        {
            _logger.LogInformation("Backup completion message found: {Message}", backupMessage);
            await _redis.DeleteAsync(BackupKey);
            return Ok(backupMessage);
        }

        // General system health (every 4 mins)
        if (ShouldCheck(ref _lastSystemCheck, now, minutes: 4, _systemLock))
        {
            _logger.LogDebug("Checking Redis for system health key...");
            var redisHealth = await _redis.GetAsync<string>(SystemHealthKey);
            if (!string.IsNullOrEmpty(redisHealth))
            {
                _logger.LogWarning("System health warning found: {Message}", redisHealth);
                await _redis.DeleteAsync(SystemHealthKey);
                return Ok(redisHealth);
            }
            else
            {
                _logger.LogDebug("No system health warnings in Redis.");
            }
        }

        _logger.LogInformation("System health check returned NoContent (all clear).");
        return NoContent();
    }

    private static async Task<bool> PingPrinterAsync(string ipAddress)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private static bool ShouldCheck(ref DateTime lastCheck, DateTime now, int minutes, object lockObj)
    {
        lock (lockObj)
        {
            if ((now - lastCheck).TotalMinutes > minutes)
            {
                lastCheck = now;
                return true;
            }
        }
        return false;
    }
}
