using Common.Data.Data;

namespace Common.Data.Repositories;

public abstract class BaseRepository
{
    public readonly GlobalTvDb database;
    protected BaseRepository(GlobalTvDb database)
    {
        this.database = database;
    }
}
