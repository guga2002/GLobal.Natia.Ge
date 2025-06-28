using Common.Domain.Models.PageVIewModel;
using Common.Persistance.Entities;

namespace Common.Persistance.Interface;

public interface IService
{
    Task<Dictionary<int, string>> GetChanellNames();
    Task<List<int>> GetPortsWhereAlarmsIsOn();
    Task<IEnumerable<SourceFromEMRModel>> GetEmrChanells(string emrcode);
    Task<List<ChannelInfo>> GetIpChanellsChannelInfoAsync();
}
