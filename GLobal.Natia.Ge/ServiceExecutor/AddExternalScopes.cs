using Common.Data.Interfaces;
using Common.Data.Repositories;
using Common.Domain.Helpers;
using Common.Domain.Interface;
using Common.Domain.Jobs;
using Common.Domain.Mapper;
using Common.Domain.Services;
using Common.Persistance.Interface;
using Common.Persistance.Services;

namespace GLobal.Natia.Ge.ServiceExecutor;

public  static class AddExternalScopes
{
    public static  IServiceCollection ActiveExternalServices(this IServiceCollection Services)
    {

        Services.AddHostedService<RefreshDataEnginner>();

        Services.AddScoped<IChanellRepository, ChanellRepository>();

        Services.AddScoped<IDesclamlerCard, DesclamlerCardRepository>();

        Services.AddScoped<IDesclambler, DesclamblerRepository>();

        Services.AddScoped<IRedisService, RedisService>();

        Services.AddScoped<IEmr60Info, Emr60InfoRepository>();

        Services.AddScoped<ISourceRepository, SourceRepository>();

        Services.AddScoped<ITranscoderRepository, TranscoderReporitory>();

        Services.AddScoped<IUniteOfWork, UniteOfWork>();

        Services.AddScoped<IAllInOneService, AllInOneService>();

        Services.AddScoped<ITranscoderService, TranscoderServices>();

        Services.AddScoped<ISatteliteFrequencyService, SatteliteFrequencyService>();

        Services.AddScoped<ISatteliteFrequency, SatteliteFrequencyRepository>();

        Services.AddScoped<IChanellServices, ChanellServices>();

        Services.AddScoped<IService, EmrServices>();

        Services.AddScoped<IsourceServices, SourceService>();

        Services.AddScoped<ITemperatureService, TemperatureService>();

        Services.AddAutoMapper(typeof(AutoMapperProfile));

        Services.AddScoped<INatiaHealthCheck, NatiaHealthCheck>();

        Services.AddScoped<IRegionsServices, RegionsServices>();

        Services.AddScoped<IGetEmrDataServices, GetEmrDataServices>();

        Services.AddScoped<RegionChecker>();

        Services.AddHostedService<VirtualEnginner>();

        Services.AddScoped<IEmrServices, EmrProvideService>();

        Services.AddScoped<IBackupService, BackupService>();

        return Services;
    }
}
