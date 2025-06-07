using Common.Persistance.Entities;

namespace Common.Persistance.Interface;

public interface IcheckRegion
{
    Task<bool> FillDatabase(List<RegionFrequency> regions);

    Task<List<RegionResponse>> GetRegionInfo();
}
