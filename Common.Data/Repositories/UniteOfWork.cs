using Common.Data.Data;
using Common.Data.Interfaces;

namespace Common.Data.Repositories;

public class UniteOfWork : BaseRepository, IUniteOfWork
{
    public UniteOfWork(GlobalTvDb database) : base(database)
    {
    }

    public IChanellRepository ChanellRepository => new ChanellRepository(database);

    public IDesclamlerCard desclamlerCardRepository => new DesclamlerCardRepository(database);

    public IDesclambler desclamblerRepository => new DesclamblerRepository(database);


    public IEmr60Info emr60InfoRepository => new Emr60InfoRepository(database);

    public ISourceRepository sourceRepository => new SourceRepository(database);

    public ITranscoderRepository transcoderRepository => new TranscoderReporitory(database);

    public ISatteliteFrequency satteliterFrequencyRepository => new SatteliteFrequencyRepository(database);

    public async Task CommitAndSavechanges()
    {
        try
        {
            await database.SaveChangesAsync();
            await database.Database.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await database.Database.RollbackTransactionAsync();
        }
    }

    public void Dispose()
    {
        database.Dispose();
    }
}
