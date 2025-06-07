using Common.Domain.Models;
using Common.Domain.Models.ViewModels;

namespace Common.Domain.Interface;

public interface ISatteliteFrequencyService : IcrudService<SatteliteFrequencyModel>
{
    Task<IEnumerable<SatteliteViewMonitoring>> GetMonitoringFrequencies();
    Task<SatteliteFrequencyModel> GetDetailsByEmrport(int PortId);
    Task<List<int>> GetAllarmsFromRegion();
}
