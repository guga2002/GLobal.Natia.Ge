using Common.Domain.Models;

namespace Common.Domain.Interface;

public interface IAllInOneService
{
    public Task<List<AllInOneModel>> SeedData();
}
