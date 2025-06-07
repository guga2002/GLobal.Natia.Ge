using Common.Domain.Interface;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;

namespace Common.Domain.Services;

public class RegionsServices : IRegionsServices
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegionsServices> _logger;

    public RegionsServices(HttpClient client, ILogger<RegionsServices> logger)
    {
        _httpClient = client;
        _logger = logger;
    }

    public async Task<int> GetCount(string emr)
    {
        var link = $"http://192.168.{emr}/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=NaN&ran=0.36219193628209";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));

        try
        {
            var response = await _httpClient.GetAsync(link, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var splitResult = content.Split(new[] { "<*1*>" }, StringSplitOptions.RemoveEmptyEntries);
                return splitResult.Length;
            }
        }
        catch (Exception ex)
        {
           _logger.LogError(ex, "Error while fetching count from EMR: {Emr}", emr);
        }

        return 0;
    }

    public async Task<bool> OpticIsOn(string ip)
    {
        try
        {
            using var ping = new Ping();

            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine($"Pinging the IP: {ip}");
                var reply = await ping.SendPingAsync(ip);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine($"Ping to {ip} failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while pinging IP: {Ip}", ip);
        }

        return false;
    }
}
