using Microsoft.Extensions.Logging;
using Quartz;
using SpeedtestMQTT.Models;
using SpeedtestMQTT.Services;

namespace SpeedtestMQTT.Jobs;

internal class RunSpeedtestJob : IJob
{
    private readonly ILogger<RunSpeedtestJob> _logger;
    private readonly ISpeedtestService _speedtestService;

    public RunSpeedtestJob(ILogger<RunSpeedtestJob> logger, ISpeedtestService speedtestService)
    {
        _logger = logger;
        _speedtestService = speedtestService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Executing RunSpeedtestJob at {Time}", DateTimeOffset.Now);

        var result = await _speedtestService.RunSpeedtestAsync();
        if (result != null)
        {
            _logger.LogDebug("Speedtest completed: {Result}", result);

            _logger.LogInformation("Speedtest Result: Ping={Ping} ms, Download={Download} Mbps, Upload={Upload} Mbps, ISP={Isp}, Server={Server}",
                result.Ping.Latency,
                result.Download.BanndwithToMbps(),
                result.Upload.BanndwithToMbps(),
                result.Isp,
                result.Server.Name);
        }
        else
        {
            _logger.LogWarning("Speedtest did not return a result.");
        }
    }
}
