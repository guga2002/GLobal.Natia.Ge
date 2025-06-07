namespace Common.Domain.Interface;

public interface IRegionsServices
{
    Task<int> GetCount(string emr);

    Task<bool> OpticIsOn(string ip);
}
