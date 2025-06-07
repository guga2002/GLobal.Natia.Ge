using System.Data;
using Common.Domain.Interface;
using Common.Persistance.Interface;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GLobal.Natia.Ge.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NatiaCoreController : ControllerBase
    {
        private readonly IEmrServices _Service;
        private readonly IRedisService redisService;
        private readonly ILogger<NatiaCoreController> _logger;

        public NatiaCoreController(IEmrServices Service, IRedisService redisService, ILogger<NatiaCoreController> logger)
        {
            this.redisService = redisService;
            _Service = Service;
            _logger = logger;
        }


        [HttpGet("natiaFeedback")]
        public async Task<IActionResult> GetNatiaFeedback()
        {
            var problems = await _Service.GetTotalProblemsWhatWeHaveRightNowOverSystem();
            int count = problems.Count;

            string natiaMessage = string.Empty;

            if (count > 14 && count < 20)
            {
                natiaMessage = "⚠️ სისტემაში შესამჩნევი პრობლემებია. გთხოვთ, მოახდინოთ შესაბამისი რეაგირება.";
            }
            else if (count > 20)
            {
                var affectedPorts = string.Join(", ", problems);
                natiaMessage =
                    $"ახლა იცი რას დავაკვირდი? დიდი პრობლემა გვაქვს! სისტემაში {count} პორტია დაზიანებული!  ჯამურად\n\n" +
                    $"გთხოვთ დაუყოვნებლივ შეამოწმოთ მოწყობილობები და გადადგათ შესაბამისი ნაბიჯები. ეს უკვე სერიოზული რამეა... {count} ალარმი არ გვეკადრება, თუნდაც ჯამური!";
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
                var regionPort = await redisService.GetAsync<List<string>>("RegionInfo") ?? new();
                var currentList = (await _Service.GetPortsWhereAlarmsIsOn()).Concat(regionPort).Distinct().OrderBy(x => x).ToList();
                var currentCsv = string.Join(",", currentList);

                var previousCsv = await redisService.GetAsync<string>("LastPortCodes:Natia") ?? "";
                var previousList = previousCsv.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();

                var lastAnnounceTicks = await redisService.GetAsync<long>("LastPortAnnounce:Natia");
                var lastAnnounceTime = new DateTime(lastAnnounceTicks);
                var timeSinceLast = DateTime.UtcNow - lastAnnounceTime;

                var difference = currentList.Except(previousList).Union(previousList.Except(currentList)).ToList();

                if (difference.Count > 1 || timeSinceLast.TotalMinutes > 5)
                {
                    await redisService.SetAsync("LastPortCodes:Natia", currentCsv);
                    await redisService.SetAsync("LastPortAnnounce:Natia", DateTime.UtcNow.Ticks);

                    _logger.LogInformation("Announcement triggered: {Count} code(s) changed or {Min} mins passed", difference.Count, timeSinceLast.TotalMinutes);
                    return Ok(currentList);
                }

                _logger.LogInformation("Ignored update. Only {Count} code(s) changed and only {Min} mins passed", difference.Count, timeSinceLast.TotalMinutes);
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
        public ActionResult<string> GetAnniversaryDates()
        {
            try
            {
                _logger.LogInformation("GetAnniversaryDates checked.");
                var time = DateTime.Now;

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
                    return Ok(message);
                }

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
}
