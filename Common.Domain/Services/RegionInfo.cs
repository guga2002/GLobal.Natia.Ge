using Common.Domain.Models.Region;

namespace Common.Domain.Services;

public class RegionInfo
{
    public async Task<List<RegionResponseViewModel>> Getinfo()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        using (var res = new HttpClient(handler))
        {
            var getres = await res.GetAsync("http://192.168.1.102:3395/api/RegionData");

            if (getres.IsSuccessStatusCode)
            {
                var stri = await getres.Content.ReadAsStringAsync();

                var rek = System.Text.Json.JsonSerializer.Deserialize<List<RegionResponseViewModel>>(stri);

                return rek;
            }
        }
        return new List<RegionResponseViewModel> { };
    }
}
