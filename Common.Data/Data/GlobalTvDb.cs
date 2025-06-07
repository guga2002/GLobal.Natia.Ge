using Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Data;

public class GlobalTvDb :DbContext
{
    public GlobalTvDb(DbContextOptions<GlobalTvDb> ops) : base(ops)
    {

    }
    public virtual DbSet<Chanell> Chanels { get; set; }

    public virtual DbSet<Desclambler> Desclamblers { get; set; }

    public virtual DbSet<DesclamlerCard> DesclamlerCards { get; set; }

    public virtual DbSet<Emr60Info> Emr60Infos { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    public virtual DbSet<Transcoder> Transcoders { get; set; }
}
