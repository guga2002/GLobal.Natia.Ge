using Common.Domain.Models.ViewModels;
using Common.Persistance.Entities;

namespace Common.Domain.Models;

public class UniteModel
{
    public UniteChanellNamesAndAlarms? chyanellnameandalarm { get; set; }
    public List<SatteliteViewMonitoring>? satelliteview { get; set; }
    public string? temperature { get; set; }
    public string? Humidity { get; set; }

    public List<ChannelInfo>? IpChanellsThatHaveProblem { get; set; }

    //public List<RegionResponseViewModel> RegionResponse { get; set; }
}
