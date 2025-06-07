using Common.Data.Entities;

namespace Common.Data.Interfaces;

public interface IEmr60Info
{
    Task<Emr60Info> GetEmrNumberBport(string port);
}
