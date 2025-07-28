using Common.Domain.Models.ViewModels;

namespace Common.Domain.Models;

public class UnitModelForApi
{
    public List<ChanellInfoModel>? ChanellInfo { get; set; }
    public List<SatteliteViewMonitoring>? SatelliteView { get; set; }
    public TemperatureView? TemperatureInfo { get; set; }
}
