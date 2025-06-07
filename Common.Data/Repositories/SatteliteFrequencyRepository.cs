using Common.Data.Data;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repositories;

public class SatteliteFrequencyRepository : BaseRepository, ISatteliteFrequency
{
    private readonly DbSet<SatteliteFrequency> frequency;
    public SatteliteFrequencyRepository(GlobalTvDb database) : base(database)
    {
        frequency = database.Set<SatteliteFrequency>();
    }

    public async Task Add(SatteliteFrequency item)
    {
        try
        {

            if (!await frequency.AnyAsync(io => io.PortIn250 == item.PortIn250))
            {
                await frequency.AddAsync(item);
                await database.SaveChangesAsync();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<IEnumerable<SatteliteFrequency>> GetAll()
    {
        return await frequency.Include(io => io.Sources).ToListAsync();
    }

    public async Task<SatteliteFrequency?> GetById(int id)
    {
        return await frequency.Include(io => io.Sources).ThenInclude(io => io.chanell).Include(io => io.Sources).FirstOrDefaultAsync(io => io.PortIn250 == id);
    }

    public async Task<SatteliteFrequency?> GetByIdIds(int id)
    {
        return await frequency.Include(io => io.Sources).ThenInclude(io => io.chanell).Include(io => io.Sources).FirstOrDefaultAsync(io => io.Id == id);
    }

    public async Task Remove(int id)
    {
        try
        {
            var res = await frequency.Include(io => io.Sources).ThenInclude(io => io.chanell).FirstOrDefaultAsync(io => io.PortIn250 == id);
            if (res is not null)
            {
                frequency.Remove(res);
                await database.SaveChangesAsync();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task Update(SatteliteFrequency item)
    {

        try
        {
            var res = await frequency.FirstOrDefaultAsync(io => io.PortIn250 == item.PortIn250);
            if (res is not null)
            {
                frequency.Entry(res).CurrentValues.SetValues(item);
                await database.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }


}
