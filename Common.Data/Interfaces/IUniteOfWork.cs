namespace Common.Data.Interfaces;

public interface IUniteOfWork : IDisposable
{
    public IChanellRepository ChanellRepository { get; }
    public IDesclamlerCard desclamlerCardRepository { get; }
    public IDesclambler desclamblerRepository { get; }
    public IEmr60Info emr60InfoRepository { get; }
    public ISourceRepository sourceRepository { get; }
    public ITranscoderRepository transcoderRepository { get; }
    public ISatteliteFrequency satteliterFrequencyRepository { get; }
    Task CommitAndSavechanges();
}
