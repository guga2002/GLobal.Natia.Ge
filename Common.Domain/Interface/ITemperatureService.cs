using Common.Domain.Models;

namespace Common.Domain.Interface;

public interface ITemperatureService
{
    Task<TemperatureModel> GetCUrrentTemperature();
}
