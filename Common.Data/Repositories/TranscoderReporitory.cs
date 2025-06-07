using Common.Data.Data;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repositories;

public class TranscoderReporitory : BaseRepository, ITranscoderRepository
{
    private readonly DbSet<Transcoder> Transcoder;
    public TranscoderReporitory(GlobalTvDb db) : base(db)
    {
        Transcoder = database.Set<Transcoder>();
    }

    public async Task Add(Transcoder item)
    {
        if (!await Transcoder.AnyAsync(io => io.Source_ID == item.Source_ID))
        {
            await Transcoder.AddAsync(item);
            await database.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Transcoder>> GetAll()
    {
        return await Transcoder.Include(io => io.Source).ThenInclude(io => io.chanell).ToListAsync();
    }

    public async Task<Transcoder> GetById(int id)
    {
        var res = await Transcoder.FirstOrDefaultAsync(io => io.Id == id);
        if (res is not null)
        {
            return res;
        }
        return null;
    }

    public async Task<Source> GetChanellIdBycardandport(int card, int port)
    {
        var res = await Transcoder.FirstOrDefaultAsync(io => io.Card == card && io.Port == port);
        if (res is not null && res.Source is not null)
        {
            return res.Source;
        }
        return null;
    }

    public async Task Remove(int id)
    {
        var res = await Transcoder.FirstOrDefaultAsync(io => io.Id == id);
        if (res is not null)
        {
            Transcoder.Remove(res);
            await database.SaveChangesAsync();
        }
    }

    public async Task Remove(int emrNumber, int card, int port)
    {
        var res = await Transcoder.FirstOrDefaultAsync(io => io.EmrNumber == emrNumber && io.Card == card && io.Port == port);
        if (res is not null)
        {
            Transcoder.Remove(res);
            await database.SaveChangesAsync();
        }
    }
    public async Task Update(Transcoder item)
    {
        var res = await Transcoder.FirstOrDefaultAsync(io => io.Source_ID == item.Source_ID);
        if (res is not null)
        {
            Transcoder.Entry(res).CurrentValues.SetValues(item);
            await database.SaveChangesAsync();
        }
    }
}
