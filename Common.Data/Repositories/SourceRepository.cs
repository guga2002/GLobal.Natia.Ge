using Common.Data.Data;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repositories;

public class SourceRepository : BaseRepository, ISourceRepository
{
    private readonly DbSet<Source> source;
    public SourceRepository(GlobalTvDb database) : base(database)
    {
        source = database.Set<Source>();
    }

    public async Task Add(Source item)
    {
        if (!await source.AnyAsync(io => io.ChanellId == item.ChanellId))
        {
            await source.AddAsync(item);
            await database.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Source>> GetAll()
    {
        return await source.Include(io => io.Transcoder).Include(io => io.Desclambler).ThenInclude(io => io.DescCard).Include(io => io.chanell).ThenInclude(io => io.Sources).ToListAsync();
    }

    public async Task<Source> GetById(int id)
    {
        var res = await source.FirstOrDefaultAsync(io => io.Id == id);
        if (res is not null)
        {
            return res;
        }
        return null;
    }

    public async Task<Source> GetBychanellName(string name)
    {
        var id = database.Chanels.FirstOrDefault(io => io.Name.ToLower() == name.ToLower());
        if (id is not null)
        {
            return await source.Where(io => io.ChanellId == id.Id).Include(io => io.chanell).FirstOrDefaultAsync();
        }
        return null;
    }

    public async Task Remove(int id)
    {
        var res = await source.FirstOrDefaultAsync(io => io.Id == id);
        if (res is not null)
        {
            source.Remove(res);
            await database.SaveChangesAsync(true);
        }
    }

    public async Task Update(Source item)
    {
        var res = await source.FirstOrDefaultAsync(io => io.Id == item.Id);
        if (res is not null)
        {
            source.Entry(res).CurrentValues.SetValues(item);
            await database.SaveChangesAsync();
        }
    }

}
