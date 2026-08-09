using Microsoft.Extensions.Hosting;
using SpeedtestMQTT.Models;

namespace SpeedtestMQTT.Services;

internal interface IMqttClientService : IHostedService
{
    Task<bool> IsConnectedAsync { get; }

    Task<bool> PublishStatusAsync(SpeedTest speedTest);
}
