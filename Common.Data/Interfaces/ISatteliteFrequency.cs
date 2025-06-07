using Common.Data.Entities;

namespace Common.Data.Interfaces;

public interface ISatteliteFrequency : BaseInterface<SatteliteFrequency>
{
    Task<SatteliteFrequency?> GetByIdIds(int id);
}
