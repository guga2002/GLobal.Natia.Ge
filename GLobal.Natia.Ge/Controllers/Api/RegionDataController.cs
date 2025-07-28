using Microsoft.AspNetCore.Mvc;
using System.Text;
using Common.Domain.Interface;
using Common.Domain.Models.Region;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class RegionDataController : ControllerBase
{
    private readonly IRedisService _redis;
    private readonly ILogger<RegionDataController> _logger;

    private const string RegionKey = "CheckRegionAndSentMessage";
    private const string BackupProblemKey = "EMRWHICHHAVEPROBLEMWHILEBACKUP";

    public RegionDataController(IRedisService redis, ILogger<RegionDataController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Checks for region issues and sends alert if backup problem exists.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        _logger.LogInformation("Fetching region data and backup problem alerts from Redis...");
        try
        {
            var regionData = await _redis.GetAsync<RegionData>(RegionKey);
            var problemsRaw = await _redis.GetAsync<string>(BackupProblemKey);

            if (!string.IsNullOrWhiteSpace(problemsRaw))
            {
                _logger.LogWarning("Backup problem detected: {Problems}", problemsRaw);
                await _redis.DeleteAsync(BackupProblemKey);

                var problemList = problemsRaw.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
                var html = GenerateHtmlTemplate(problemList);
                _logger.LogInformation("Generated HTML alert template for {Count} affected devices.", problemList.Count);
                return Ok(html);
            }

            if (!string.IsNullOrWhiteSpace(regionData?.Text))
            {
                _logger.LogInformation("Region-specific alert found: {Message}", regionData.Text);
                await _redis.DeleteAsync(RegionKey);
                return Ok(regionData.Text);
            }

            _logger.LogInformation("No region data or backup problems found in Redis.");
            return NotFound("No region data or backup problems found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching region or backup data.");
            return StatusCode(500, new
            {
                error = "დაფიქსირდა შეცდომა მონაცემების წამოღებისას",
                details = ex.Message,
                trace = ex.StackTrace
            });
        }
    }

    private string GenerateHtmlTemplate(List<string> ips)
    {
        _logger.LogDebug("Generating HTML backup failure report for {Count} IP(s): {IPs}", ips.Count, string.Join(", ", ips));

        var sb = new StringBuilder();

        sb.AppendLine(@"<!DOCTYPE html>
<html lang=""ka"">
<head>
    <meta charset=""UTF-8"">
    <title>პრობლემა სისტემაში</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; }
        .header, .footer { background-color: #f4f4f4; padding: 10px; text-align: center; border: 1px solid #ddd; }
        .content { margin: 20px; }
    </style>
</head>
<body>
    <div class=""header""><h1>პრობლემა გვაქვს ბექაფის აღების დროს!</h1></div>
    <div class=""content"">
        <p>გამარჯობა,</p>
        <p>მინდა შეგატყობინო როდესაც ბექაფს ვიღებდი პრობლემა შემექმნა შემდეგ მოწყობილობებზე:</p>
        <ul>");

        foreach (var ip in ips)
            sb.AppendLine($"<li>EMR: {ip}</li>");

        sb.AppendLine(@"</ul>
        <p>შესაბამისად ვერ ავიღე ბექაფი, იქნებ გადაამოწმო ფიზიკურად თ არის მოწყობილობა მწყობრში, წვდომა არ მაქვს!</p>
        <p>დამატებით დეტალებისთვის გადაამოწმე სერვერზე.</p>
    </div>
    <div class=""footer"">&copy; 2024 Natia Jandagishvili</div>
</body>
</html>");

        return sb.ToString();
    }
}
