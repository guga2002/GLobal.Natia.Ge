using System.Text.Json;
using Common.Domain.Interface;
using Common.Domain.Models;

namespace Common.Domain.Services;

public class TemperatureService : ITemperatureService
{
    private const string serverUrl = "http://192.168.0.79:2000";
    public async Task<TemperatureModel> GetCUrrentTemperature()
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetAsync($"{serverUrl}/api/Temprature/GetCurrentTemperature");
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            var res=JsonSerializer.Deserialize<TemperatureModel>(responseData);
            if (res == null)
            {
                return new TemperatureModel { humidity = "N/A", temperature = "N/A" };
            }
            return res;
        }
        catch (Exception ex)
        {
            return new TemperatureModel { humidity = "N/A", temperature = "N/A" };
        }
    }
}
