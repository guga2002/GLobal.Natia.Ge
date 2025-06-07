using Common.Data.Data;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repositories;

public class Emr60InfoRepository : BaseRepository, IEmr60Info
{
    private readonly DbSet<Emr60Info> _dbSet;
    public Emr60InfoRepository(GlobalTvDb db) : base(db)
    {
        _dbSet = database.Set<Emr60Info>();
    }

    public async Task<Emr60Info> GetEmrNumberBport(string port)
    {
        var res = await _dbSet.FirstOrDefaultAsync(io => io.Port == port);
        if (res is not null)
        {
            return res;
        }
        return null;
    }
}
