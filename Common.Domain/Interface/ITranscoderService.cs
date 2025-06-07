using Common.Domain.Models.ViewModels;

namespace Common.Domain.Interface;

public interface ITranscoderService : IcrudService<TranscoderViewModel>
{
    Task Remove(int emrNumber, int card, int port);
    Task<TranscoderViewModel> GetDropDownList();
}
