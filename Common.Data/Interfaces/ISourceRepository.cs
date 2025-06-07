using Common.Data.Entities;

namespace Common.Data.Interfaces;

public interface ISourceRepository : BaseInterface<Source>
{
    Task<Source> GetBychanellName(string name);
}
