using Common.Domain.Models;

namespace Common.Domain.Interface;

public interface IsourceServices : IcrudService<SourceModel>
{
    Task<SourceModel> GetDropDOwnListData();
}
