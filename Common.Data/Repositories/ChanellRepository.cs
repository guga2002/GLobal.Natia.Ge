using Common.Data.Data;
using Common.Data.Entities;
using Common.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Common.Data.Repositories
{
    public class ChanellRepository : BaseRepository, IChanellRepository
    {
        private readonly DbSet<Chanell> chanells;

        public ChanellRepository(GlobalTvDb db) : base(db)
        {
            chanells = database.Set<Chanell>();
        }

        public async Task<bool> addSource(string name, Source sr)
        {
            var chanel = chanells.Where(io => io.Name == name).FirstOrDefault();
            if (chanel is not null)
            {
                if (!chanel.Sources.Any(io => io.EmrNumber == sr.EmrNumber && io.port == sr.port && io.card == sr.card))
                {
                    if (!chanel.Sources.Any(io => io.sourceName == sr.sourceName))
                    {
                        chanel.Sources.Add(sr);
                        await database.SaveChangesAsync();
                        return true;
                    }
                }
            }
            return false;
        }
        public async Task Add(Chanell item)
        {
            if (!await chanells.AnyAsync(IO => IO.Name == item.Name || IO.Id == item.Id))
            {
                await chanells.AddAsync(item);
                await database.SaveChangesAsync();
            }
            else
            {
                var chan = await chanells.FirstOrDefaultAsync(IO => IO.Id == item.Id);
                if (chan is not null)
                {
                    chan.Name = item.Name;
                    await database.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<Chanell>> GetAll()
        {
            return await chanells.Include(io => io.Sources).AsNoTracking().ToListAsync();
        }

        public async Task<Chanell?> GetById(int id)
        {
            return await chanells.Include(io => io.Sources).AsNoTracking().FirstOrDefaultAsync(io => io.Id == id);
        }

        public async Task Remove(int id)
        {
            var chan = await chanells.FirstOrDefaultAsync(io => io.Id == id);
            if (chan != null)
            {
                chanells.Remove(chan);
                await database.SaveChangesAsync();
            }
        }
        public async Task Remove(string name)
        {
            var chan = await chanells.Include(io => io.Sources).FirstOrDefaultAsync(io => io.Name == name);
            if (chan != null)
            {
                chanells.Remove(chan);
                await database.SaveChangesAsync();
            }
        }
        public async Task Update(Chanell item)
        {
            var cha = await chanells.FirstOrDefaultAsync(io => io.Name == item.Name);

            if (cha != null)
            {
                chanells.Entry(cha).CurrentValues.SetValues(item);
                await database.SaveChangesAsync();
            }
        }

        public async Task<Chanell> GetCHanellByName(string name)
        {

            return await chanells.FirstOrDefaultAsync(io => io.Name.ToLower() == name.ToLower());
        }
    }
}
