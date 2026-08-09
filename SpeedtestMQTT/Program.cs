using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using SpeedtestMQTT.Jobs;
using SpeedtestMQTT.Models;
using SpeedtestMQTT.Services;

const string VersionArgs = "--version";
const string SerilogOutputTemplate = "[{Timestamp:HH:mm:ss} {SourceContext} [{Level}] {Message}{NewLine}{Exception}";

var printVersion = args.Any(x => x == VersionArgs);
if (printVersion)
{
    Console.WriteLine(GetVersion());
    return;
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(GetConfiguration(args))
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: SerilogOutputTemplate)
    .CreateBootstrapLogger();

HostApplicationBuilderSettings settings = new()
{
    Args = args,
    Configuration = new ConfigurationManager(),
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production",
};

HostApplicationBuilder builder = Host.CreateApplicationBuilder(settings);

Log.ForContext<Program>().Information("Starting SpeedtestMQTT {Version}", GetVersion());

builder.Services.AddOptions<MqttSettings>()
    .BindConfiguration(MqttSettings.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: SerilogOutputTemplate);
});

builder.Services.AddSingleton<ISpeedtestService, SpeedtestService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddHostedService(serviceProvider => (IMqttClientService)serviceProvider.GetRequiredService<IMqttClientService>());

builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey(nameof(RunSpeedtestJob));
    options
    .AddJob<RunSpeedtestJob>(jobKey, (IJobConfigurator job) => { }) // Explicitly specify the delegate type
    .AddTrigger(trigger =>
        trigger
            .ForJob(jobKey)            
            .WithCronSchedule("0 0 0,12 * * ?").StartNow()); // Every 12 hours
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

IHost host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
    IScheduler scheduler = await schedulerFactory.GetScheduler();
    JobKey jobKey = new JobKey(nameof(RunSpeedtestJob));
    await scheduler.TriggerJob(jobKey);
}

await host.RunAsync();

static string GetVersion()
{
    Assembly currentAssembly = typeof(Program).Assembly;
    if (currentAssembly == null)
    {
        currentAssembly = Assembly.GetCallingAssembly();
    }
    var version = $"{currentAssembly.GetName().Version!.Major}.{currentAssembly.GetName().Version!.Minor}.{currentAssembly.GetName().Version!.Build}";
    return version ?? "?.?.?";
}

static IConfiguration GetConfiguration(string[] args)
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();
    return configuration;
}
