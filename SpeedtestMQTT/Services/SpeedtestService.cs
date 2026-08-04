using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SpeedtestMQTT.Models;

namespace SpeedtestMQTT.Services;

internal class SpeedtestService : ISpeedtestService
{
    private readonly ILogger<SpeedtestService> _logger;

    public SpeedtestService(ILogger<SpeedtestService> logger)
    {
        _logger = logger;
    }

    public async Task<SpeedtestResult?> RunSpeedtestAsync(CancellationToken cancellationToken = default)
    {
        return await RunRaspberryPiSpeedtestAsync(cancellationToken);
    }

    private async Task<SpeedtestResult?> RunRaspberryPiSpeedtestAsync(CancellationToken cancellationToken = default)
    {
        string speedtestPath = Path.Combine(Directory.GetCurrentDirectory(), "clis", "speedtest.exe");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            speedtestPath = Path.Combine(Directory.GetCurrentDirectory(), "clis", "speedtest");
        }
        var startInfo = new ProcessStartInfo
        {
            FileName = speedtestPath,
            // --accept-license & --accept-gdpr are REQUIRED for automated scripts on headless Pis
            Arguments = "--format=json --accept-license --accept-gdpr",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null) return null;

            // Stream reading to handle potential memory constraints on low-RAM Pis
            string jsonOutput = await process.StandardOutput.ReadToEndAsync();
            string errorOutput = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                _logger.LogError("[CLI Error]: {ErrorOutput}", errorOutput);
                return null;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SpeedtestResult>(jsonOutput, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Execution Failed]: {Message}", ex.Message);
            return null;
        }
    }
}
