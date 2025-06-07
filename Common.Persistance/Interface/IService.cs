using Common.Domain.Models.PageVIewModel;

namespace Common.Persistance.Interface;

public interface IService
{
    Task<Dictionary<int, string>> GetChanellNames();
    Task<List<int>> GetPortsWhereAlarmsIsOn();
    Task<IEnumerable<SourceFromEMRModel>> GetEmrChanells(string emrcode);
}
