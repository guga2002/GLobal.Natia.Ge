
using Common.Data.Entities;

namespace Common.Data.Interfaces;

public interface IChanellRepository : BaseInterface<Chanell>
{
    Task Remove(string name);
    Task<Chanell> GetCHanellByName(string name);
    Task<bool> addSource(string name, Source sr);
}
