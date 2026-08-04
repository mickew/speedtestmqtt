using SpeedtestMQTT.Models;

namespace SpeedtestMQTT.Services;

internal interface ISpeedtestService
{
    Task<SpeedtestResult?> RunSpeedtestAsync(CancellationToken cancellationToken = default);
}
