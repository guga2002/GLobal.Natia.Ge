using Common.Data.Entities;

namespace Common.Data.Interfaces;

public interface ITranscoderRepository : BaseInterface<Transcoder>
{
    Task<Source> GetChanellIdBycardandport(int card, int port);
    Task Remove(int emrNumber, int card, int port);
}
