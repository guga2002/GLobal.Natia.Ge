using System.Net;
using Common.Data.Data;
using Common.Data.Interfaces;
using Common.Data.Repositories;
using Common.Domain.Helpers;
using Common.Domain.Interface;
using Common.Domain.Jobs;
using Common.Domain.Mapper;
using Common.Domain.Services;
using Common.Domain.SignalR;
using Common.Persistance.Interface;
using Common.Persistance.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddControllers();

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(3395);
    options.Listen(IPAddress.Any, 3999, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});



builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("127.0.0.1:6379"));

builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddDbContext<GlobalTvDb>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("GlobalCOnnection"));
});

builder.Services.AddHttpClient();

builder.Services.AddSignalR();

builder.Services.AddHostedService<RefreshDataEnginner>();

builder.Services.AddScoped<IChanellRepository, ChanellRepository>();

builder.Services.AddScoped<IDesclamlerCard, DesclamlerCardRepository>();

builder.Services.AddScoped<IDesclambler, DesclamblerRepository>();

builder.Services.AddScoped<IEmr60Info, Emr60InfoRepository>();

builder.Services.AddScoped<ISourceRepository, SourceRepository>();

builder.Services.AddScoped<ITranscoderRepository, TranscoderReporitory>();

builder.Services.AddScoped<IUniteOfWork, UniteOfWork>();

builder.Services.AddScoped<IAllInOneService, AllInOneService>();

builder.Services.AddScoped<ITranscoderService, TranscoderServices>();

builder.Services.AddScoped<ISatteliteFrequencyService, SatteliteFrequencyService>();

builder.Services.AddScoped<ISatteliteFrequency, SatteliteFrequencyRepository>();

builder.Services.AddScoped<IChanellServices, ChanellServices>();

builder.Services.AddScoped<IService, EmrServices>();

builder.Services.AddScoped<IsourceServices, SourceService>();

builder.Services.AddScoped<ITemperatureService, TemperatureService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddScoped<INatiaHealthCheck, NatiaHealthCheck>();

builder.Services.AddScoped<IRegionsServices, RegionsServices>();

builder.Services.AddScoped<IGetEmrDataServices, GetEmrDataServices>();

builder.Services.AddScoped<RegionChecker>();


builder.Services.AddHostedService<VirtualEnginner>();

builder.Services.AddScoped<IEmrServices, EmrProvideService>();


builder.Services.AddScoped<IBackupService, BackupService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true);
    });
});

builder.Services.AddSwaggerGen();

var app = builder.Build();



app.MapControllers();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.MapHub<UniteMonitoringHub>("/uniteHub");
app.MapDefaultControllerRoute();
app.MapControllerRoute(
    name: "default",
   pattern: "{controller=Unite}/{action=Index}");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Global TV API V1");
    c.RoutePrefix = string.Empty;
});

app.Run();
