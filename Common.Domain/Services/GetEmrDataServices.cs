using Common.Domain.Contexts;
using Common.Domain.Interface;
using Microsoft.Extensions.Logging;

namespace Common.Domain.Services;

public class GetEmrDataServices : IGetEmrDataServices
{
    private readonly RegionStorage con;
    private readonly HttpClient client;
    private readonly ILogger<GetEmrDataServices> logger;
    public static int alarmcount { get; set; } = 0;
    private string status { get; set; }

    public GetEmrDataServices(HttpClient client, ILogger<GetEmrDataServices> logger)
    {
        con = new RegionStorage();
        status = string.Empty;
        this.client = client;
        this.logger = logger;
    }

    public async Task<string> Start()
    {
        logger.LogInformation("Starting EMR alarm check process.");
        return await checknow();
    }

    private async Task<string> checknow()
    {
        Destruct();
        await LoadAlarms();

        int alarmnew = con.Telavi.Count + con.Qutaisi.Count + con.Foti.Count + con.Gori.Count;
        int sxvaoba = alarmnew - alarmcount;

        logger.LogInformation("Previous alarm count: {OldCount}, Current alarm count: {NewCount}, Difference: {Difference}",
                              alarmcount, alarmnew, sxvaoba);

        if (alarmcount == 0 || alarmnew == 0)
        {
            alarmcount = alarmnew;
            string result = GetString();
            logger.LogInformation("Initial alarm check or all cleared. Returning report.");
            return result;
        }
        else if (Math.Abs(sxvaoba) >= 2)
        {
            alarmcount = alarmnew;
            string result = GetString();
            logger.LogInformation("Significant alarm count change detected. Returning report.");
            return result;
        }
        else
        {
            alarmcount = alarmnew;
            logger.LogInformation("No significant change in alarm count detected.");
            return string.Empty;
        }
    }

    public void Destruct()
    {
        logger.LogInformation("Resetting region alarm storage.");
        con.Qutaisi.Clear();
        con.Telavi.Clear();
        con.Foti.Clear();
        con.Gori.Clear();
    }

    private string GetString()
    {
        logger.LogInformation("Generating alarm HTML report.");
        status = "";

        status += "<div style=\"text-align: center;\"><b style=\"color:green;\"><span style=\"font-size: 16px;\">ქუთაისის სადგური</span></b></div><br/>";
        foreach (var item in con.Qutaisi)
            status += $"<span style=\"color: red; font-size: 11px;\">{item}</span><br/>";

        status += "<div style=\"text-align: center;\"><b style=\"color:green;\"><span style=\"font-size: 16px;\">თელავის სადგური</span></b></div><br/>";
        foreach (var item in con.Telavi)
        {
            if (!item.Contains("Card 3 Port 1"))
                status += $"<br/><span style=\"color: red; font-size: 11px;\">{item}</span><br/>";
        }

        status += "<div style=\"text-align: center;\"><b style=\"color:green;\"><span style=\"font-size: 16px;\">ფოთის სადგური</span></b></div><br/>";
        foreach (var item in con.Foti)
            status += $"<span style=\"color: red; font-size: 11px;\">{item}</span><br/><br/>";

        status += "<div style=\"text-align: center;\"><b style=\"color:green;\"><span style=\"font-size: 16px;\">გორის სადგური</span></b></div><br/><br/>";
        foreach (var item in con.Gori)
            status += $"<br/><span style=\"color: red; font-size:11px;\">{item}</span><br/>";

        return status;
    }

    private async Task LoadAlarms()
    {
        logger.LogInformation("Loading alarms from all EMR stations.");
        con.Telavi.AddRange(await GetInfo("25.10"));
        con.Qutaisi.AddRange(await GetInfo("15.10"));
        con.Gori.AddRange(await GetInfo("13.10"));
        con.Qutaisi.AddRange(await GetInfo("15.30"));
        con.Foti.AddRange(await GetInfo("14.10"));
    }

    private async Task<List<string>> GetInfo(string dat)
    {
        List<string> lst = new();
        string link = $"http://192.168.{dat}/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=NaN&ran=0.36219193628209";
        logger.LogInformation("Requesting alarm data from {IpSegment}", dat);

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var res = await client.GetAsync(link, cts.Token);

            if (res.IsSuccessStatusCode)
            {
                var re = await res.Content.ReadAsStringAsync();
                var splitResult = re.Split(new[] { "<*1*>", "<html>", "<html/>", "</html>" }, StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> addedItems = new();

                foreach (var item in splitResult.Where(io => !io.Contains("Card 7 GbE 3")).OrderByDescending(io => io.Contains("Demodulator Unlock")))
                {
                    if (item.Contains("4-Ch.DVB-S2 Demod Card"))
                    {
                        var axali = item.Split("Demod");
                        if (axali.Length >= 2 && !string.IsNullOrWhiteSpace(axali[1]) && addedItems.Add(axali[1]))
                        {
                            lst.Add(axali[1]);
                            logger.LogDebug("Added Demod alert: {Alert}", axali[1]);
                        }
                    }
                    else if (item.Contains("Main GbE Card"))
                    {
                        var axali = item.Split("(C451E):")[1];

                        bool containsPort = con.Ports.Any(port => axali.IndexOf(port, StringComparison.OrdinalIgnoreCase) >= 0);
                        if (!containsPort && addedItems.Add(axali))
                        {
                            lst.Add(axali);
                            logger.LogDebug("Added GbE alert: {Alert}", axali);
                        }
                    }
                    else
                    {
                        if (addedItems.Add(item))
                        {
                            lst.Add(item);
                            logger.LogDebug("Added Generic alert: {Alert}", item);
                        }
                    }
                }
                lst.Add("\n");
            }
            else
            {
                logger.LogWarning("Failed to fetch alarm data from {IpSegment}. Status Code: {StatusCode}", dat, res.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while requesting alarm data from {IpSegment}", dat);
        }

        return lst;
    }
}
