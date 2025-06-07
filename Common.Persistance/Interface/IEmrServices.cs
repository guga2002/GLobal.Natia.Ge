using Common.Persistance.Entities;

namespace Common.Persistance.Interface;

public interface IEmrServices
{
    Task<List<string>> GetPortsWhereAlarmsIsOn();

    Task<List<ActiveCard>> GetCardsWhichNeedToBeActivate(string Emr);

    Task<List<string>> GetTotalProblemsWhatWeHaveRightNowOverSystem();
}
