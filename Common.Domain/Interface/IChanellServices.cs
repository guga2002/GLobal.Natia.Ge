using Common.Data.Entities;
using Common.Domain.Models;

namespace Common.Domain.Interface;

public interface IChanellServices : IcrudService<ChanellModel>
{
    Task<bool> DeleteByName(string Name);
    Task<Common.Data.Entities.Chanell> GetCHanellByName(string name);
    Task<bool> addSource(string name, Source sr);
}
