using Common.Domain.Interface;

namespace Common.Domain.Services;

public class TemperatureService : ITemperatureService
{
    private static readonly HttpClient client = new HttpClient();
    private const string serverUrl = "http://192.168.100.104:8080";
    public async Task<(string, string)> GetCUrrentTemperature()
    {
        try
        {
            var response = await client.GetAsync($"{serverUrl}/monitoring");
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            var res = responseData.Split(new string[] { "<div id=\"temperature\" class=\"temperature\">", "°C</div>" }, StringSplitOptions.None)[1];
            var Humidity = responseData.Split(new string[] { "<div class=\"humidity\">", "%</div>" }, StringSplitOptions.None)[1];
            return (res, Humidity);
        }
        catch (Exception ex)
        {
            return ("შეცდომა დემპერატურის წამოღებისას", "");
        }
    }
}
