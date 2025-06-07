namespace Common.Domain.Interface;

public interface ITemperatureService
{
    Task<(string, string)> GetCUrrentTemperature();
}
