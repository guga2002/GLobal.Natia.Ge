using System.Data;
using Common.Domain.Interface;
using Common.Domain.Models.ViewModels;
using Common.Domain.Models;
using Common.Persistance.Extensions;
using Common.Persistance.Interface;
using Common.Persistance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class NatiaCoreController : ControllerBase
{
    private readonly IEmrServices _Service;
    private readonly IRedisService redisService;
    private readonly ILogger<NatiaCoreController> _logger;
    private readonly IService chanells;

    public NatiaCoreController(IEmrServices Service, IRedisService redisService, ILogger<NatiaCoreController> logger, IService chanells)
    {
        this.redisService = redisService;
        _Service = Service;
        _logger = logger;
        this.chanells = chanells;
    }

    [HttpGet("GetChanellNames")]
    public async Task<IActionResult> GetChanellNames(string emr)
    {
        _logger.LogInformation("Fetching channel names for EMR: {Emr}", emr);
        var res = await chanells.GetEmrChanells(emr);
        _logger.LogInformation("Retrieved {Count} channel names for EMR: {Emr}", res?.Count() ?? 0, emr);
        return Ok(res);
    }

    [HttpGet("GetProblematicChanells")]
    public async Task<IActionResult> GetProblematicChanells(string emr)
    {
        _logger.LogInformation("Fetching problematic channels for EMR: {Emr}", emr);
        var redis = await redisService.GetAsync<List<MulticastAnalysisResult>>("SystemStreamInfo");
        _logger.LogInformation("Retrieved {Count} problematic channels from Redis for EMR: {Emr}", redis?.Count ?? 0, emr);
        return Ok(redis);
    }

    [HttpGet("natiaFeedback")]
    public async Task<IActionResult> GetNatiaFeedback()
    {
        _logger.LogInformation("Fetching Natia feedback (current problems in system).");
        var problems = await _Service.GetTotalProblemsWhatWeHaveRightNowOverSystem();
        int count = problems.Count;
        _logger.LogInformation("System currently has {Count} problems.", count);

        string natiaMessage = string.Empty;

        if (count > 14 && count < 20)
        {
            natiaMessage = "⚠️ სისტემაში შესამჩნევი პრობლემებია. გთხოვთ, მოახდინოთ შესაბამისი რეაგირება.";
            _logger.LogWarning("Moderate system alert: {Count} problems detected.", count);
        }
        else if (count > 20)
        {
            var affectedPorts = string.Join(", ", problems);
            natiaMessage =
                $"ახლა იცი რას დავაკვირდი? დიდი პრობლემა გვაქვს! სისტემაში {count} პორტია დაზიანებული!  ჯამურად\n\n" +
                $"გთხოვთ დაუყოვნებლივ შეამოწმოთ მოწყობილობები და გადადგათ შესაბამისი ნაბიჯები. ეს უკვე სერიოზული რამეა... {count} ალარმი არ გვეკადრება, თუნდაც ჯამური!";
            _logger.LogError("Critical system alert: {Count} problems detected. Ports: {Ports}", count, affectedPorts);
        }
        else
        {
            _logger.LogInformation("System is within normal problem thresholds ({Count}).", count);
        }

        return Ok(natiaMessage);
    }

    [HttpGet]
    [Route("info")]
    [SwaggerOperation(Summary = "Get information about active alarm ports", Description = "Fetches a list of ports where alarms are active. Only speaks if 2+ ports changed or 5 minutes passed.", OperationId = "GetInfo")]
    [SwaggerResponse(200, "Returns the list of ports if changes are significant, otherwise a static response.", typeof(List<string>))]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<ActionResult<List<string>>> GetInfo()
    {
        try
        {
            _logger.LogInformation("Checking Natia port alarm info...");
            var regionPort = await redisService.GetAsync<List<string>>("RegionInfo") ?? new();
            var currentList = (await _Service.GetPortsWhereAlarmsIsOn()).Concat(regionPort).Distinct().OrderBy(x => x).ToList();
            var currentCsv = string.Join(",", currentList);

            var previousCsv = await redisService.GetAsync<string>("LastPortCodes:Natia") ?? "";
            var previousList = previousCsv.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

            var lastAnnounceTicks = await redisService.GetAsync<long>("LastPortAnnounce:Natia");
            var lastAnnounceTime = new DateTime(lastAnnounceTicks);
            var timeSinceLast = DateTime.UtcNow - lastAnnounceTime;

            var difference = currentList.Except(previousList).Union(previousList.Except(currentList)).ToList();

            if (difference.Count > 1)
            {
                _logger.LogInformation("Triggering Natia announcement. Changed codes: {Count}, Time since last: {Minutes} mins",
                    difference.Count, timeSinceLast.TotalMinutes);

                await redisService.SetAsync("LastPortCodes:Natia", currentCsv);
                await redisService.SetAsync("LastPortAnnounce:Natia", DateTime.UtcNow.Ticks);

                return Ok(currentList);
            }

            _logger.LogInformation("Announcement skipped. Changed codes: {Count}, Time since last: {Minutes} mins",
                difference.Count, timeSinceLast.TotalMinutes);

            return Ok(new List<string> { "300000" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetInfo.");
            return Ok(new List<string> { "900000" });
        }
    }

    [HttpGet("GetAnniversaryDates")]
    [SwaggerOperation(Summary = "Get information about GetAnniversaryDates", Description = "Fetches a greeting message if today's date matches a significant holiday.", OperationId = "GetAnniversaryDates")]
    [SwaggerResponse(200, "Returns the anniversary message if today's date matches.")]
    [SwaggerResponse(204, "No anniversary message for today's date.")]
    [SwaggerResponse(500, "An error occurred while processing the request.")]
    public async Task<ActionResult<string>> GetAnniversaryDates()
    {
        try
        {
            _logger.LogInformation("Checking for anniversary dates and holiday greetings...");
            var time = DateTime.Now;

            var url = new Uri("http://192.168.0.13:9999/api/ExcelData/GetRobotSay");
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("RobotSay API responded successfully.");
                var result = await response.Content.ReadAsStringAsync();

                var actres = JsonConvert.DeserializeObject<List<string>>(result);

                if (actres is not null && actres.Count > 3)
                {
                    var joined = string.Join(", ", actres.Take(3));
                    _logger.LogInformation("RobotSay returned a valid list: {Joined}", joined);
                    return Ok(joined);
                }
                else
                {
                    _logger.LogInformation("RobotSay returned insufficient data (Count={Count}).", actres?.Count ?? 0);
                }
            }
            else
            {
                _logger.LogWarning("RobotSay API request failed with status code: {StatusCode}", response.StatusCode);
            }

            var holidays = new Dictionary<(int, int), string>
            {
                [(1, 1)] = "გილოცავთ ახალ წელს! ახალი დასაწყისი არის ახალი შესაძლებლობებისა და იმედების დრო...",
                [(1, 7)] = "გილოცავთ შობას! ეს არის სიყვარულის, მშვიდობისა და რწმენის დღე...",
                [(1, 19)] = "გილოცავთ ნათლისღებას! ეს წმინდა დღე გვახსენებს სულიერი განწმენდის მნიშვნელობას...",
                [(3, 3)] = "გილოცავთ დედების დღეს! მოდით, პატივი მივაგოთ მათ თავდადებისა და უსაზღვრო სიყვარულისთვის.",
                [(4, 9)] = "ეროვნული ერთიანობის დღე არის ჩვენი ქვეყნის ძალისა და სიძლიერის სიმბოლო...",
                [(5, 9)] = "გილოცავთ გამარჯვების დღეს! მოდით, პატივი მივაგოთ გმირებს, რომლებმაც თავიანთი სიცოცხლე გასწირეს თავისუფლებისთვის.",
                [(5, 26)] = "გილოცავთ დამოუკიდებლობის დღეს! ეს დღე გვახსენებს ჩვენი ქვეყნის თავისუფლების მნიშვნელობას.",
                [(6, 1)] = "გილოცავთ წმინდა ნინოს დღეს! რწმენის სიძლიერე, თავდადება და სიყვარული.",
                [(8, 28)] = "გილოცავთ მარიამობას! ღვთისმშობლის ეს წმინდა დღე გვასწავლის სიკეთესა და ზრუნვას.",
                [(10, 14)] = "გილოცავთ სვეტიცხოვლობას! ეს წმინდა დღე ჩვენი სულიერი სიძლიერის ნიშანია.",
                [(11, 23)] = "გილოცავთ გიორგობას და ვარდების რევოლუციის დღეს! სიმამაცისა და თავისუფლების სიმბოლო."
            };

            if (holidays.TryGetValue((time.Month, time.Day), out var message) && IsCurrentTimeInRange())
            {
                _logger.LogInformation("Anniversary date detected: {Message}", message);
                return Ok(message);
            }

            _logger.LogInformation("No matching anniversary date for today ({Date}).", time.ToShortDateString());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in GetAnniversaryDates.");
            return StatusCode(500);
        }
    }

    private bool IsCurrentTimeInRange()
    {
        TimeSpan now = DateTime.Now.TimeOfDay;
        return now >= new TimeSpan(9, 0, 0) && now <= new TimeSpan(9, 1, 0) ||
               now >= new TimeSpan(21, 0, 0) && now <= new TimeSpan(21, 1, 0);
    }
}
