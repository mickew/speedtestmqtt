using Microsoft.Extensions.Logging;
using Quartz;
using SpeedtestMQTT.Models;
using SpeedtestMQTT.Services;

namespace SpeedtestMQTT.Jobs;

internal class RunSpeedtestJob : IJob
{
    private readonly ILogger<RunSpeedtestJob> _logger;
    private readonly ISpeedtestService _speedtestService;
    private readonly IMqttClientService _mqttClientService;

    public RunSpeedtestJob(ILogger<RunSpeedtestJob> logger, ISpeedtestService speedtestService, IMqttClientService mqttClientService)
    {
        _logger = logger;
        _speedtestService = speedtestService;
        _mqttClientService = mqttClientService;
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
            if (await _mqttClientService.IsConnectedAsync)
            {
                SpeedTest speedTest = new SpeedTest
                {
                    PingLatency = result.Ping.Latency,
                    DownloadSpeedMbps = result.Download.BanndwithToMbps(),
                    UploadSpeedMbps = result.Upload.BanndwithToMbps(),
                    ISP = result.Isp,
                    ServerName = result.Server.Name
                };
                await _mqttClientService.PublishStatusAsync(speedTest);
            }
            else
            {
                _logger.LogWarning("MQTT client is not connected. Skipping publish.");

            }
        }
        else
        {
            _logger.LogWarning("Speedtest did not return a result.");
        }
    }
}
