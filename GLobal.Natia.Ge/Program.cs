using System.Net;
using Common.Data.Data;
using Common.Domain.SignalR;
using GLobal.Natia.Ge.ServiceExecutor;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using NLog;
using NLog.Web;

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    logger.Info("Starting Global TV API...");

    var builder = WebApplication.CreateBuilder(args);

    // -----------------------
    // Configure NLog
    // -----------------------
    builder.Logging.ClearProviders();       // Remove default loggers
    builder.Host.UseNLog();                 // Use NLog for DI logging

    // -----------------------
    // Configure Services
    // -----------------------

    builder.Services.AddControllersWithViews()
        .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

    // Database (SQL Server)
    builder.Services.AddDbContext<GlobalTvDb>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("GlobalCOnnection"));
    });

    // Redis
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect("127.0.0.1:6379"));

    // HTTP Client & SignalR
    builder.Services.AddHttpClient();
    builder.Services.AddSignalR();

    // API Explorer + Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // CORS (Allow all origins)
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowed(_ => true);
        });
    });

    // Register external services (from your custom method)
    builder.Services.ActiveExternalServices();

    // Configure Kestrel for HTTP & HTTPS
    builder.WebHost.UseKestrel(options =>
    {
        options.ListenAnyIP(3395); // HTTP
    });

    var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Global TV API V1");
        c.RoutePrefix = string.Empty; 
    });

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors();

    app.MapHub<UniteMonitoringHub>("/uniteHub");
    app.MapControllers();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Unite}/{action=Index}");

    logger.Info("Global TV API started successfully.");
    await app.RunAsync();
}
catch (Exception ex)
{
    // Log fatal errors
    logger.Error(ex, "Application stopped due to an exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
