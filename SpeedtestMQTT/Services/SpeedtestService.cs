using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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

    //private async Task<SpeedtestResult?> RunRaspberryPiSpeedtestAsync(CancellationToken cancellationToken = default)
    //{
    //    string speedtestPath = Path.Combine(Directory.GetCurrentDirectory(), "clis", "speedtest.exe");
    //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    //    {
    //        speedtestPath = Path.Combine(Directory.GetCurrentDirectory(), "clis", "speedtest");
    //    }
    //    _logger.LogInformation("Using speedtest CLI at: {SpeedtestPath}", speedtestPath);

    //    var startInfo = new ProcessStartInfo
    //    {
    //        FileName = speedtestPath,
    //        // --accept-license & --accept-gdpr are REQUIRED for automated scripts on headless Pis
    //        Arguments = "--format=json --accept-license --accept-gdpr",
    //        RedirectStandardOutput = true,
    //        RedirectStandardError = true,
    //        UseShellExecute = false,
    //        CreateNoWindow = true
    //    };

    //    try
    //    {
    //        _logger.LogInformation("Starting speedtest process...{StartInfo}", startInfo);
    //        using var process = Process.Start(startInfo);
    //        if (process == null) return null;

    //        // Stream reading to handle potential memory constraints on low-RAM Pis
    //        string jsonOutput = process.StandardOutput.ReadToEnd();
    //        string errorOutput = process.StandardError.ReadToEnd();

    //        await process.WaitForExitAsync(cancellationToken);

    //        if (process.ExitCode != 0)
    //        {
    //            _logger.LogError("[CLI Error]: Speedtest CLI exited with code {ExitCode}. Error Output: {ErrorOutput}", process.ExitCode, errorOutput);
    //            return null;
    //        }

    //        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    //        return await JsonSerializer.DeserializeAsync<SpeedtestResult>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonOutput)), options, cancellationToken);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "[Execution Failed]: {Message}", ex.Message);
    //        return null;
    //    }
    //}

    private Task<SpeedtestResult?> RunRaspberryPiSpeedtestAsync(CancellationToken cancellationToken = default)
    {
        string speedtestPath = Path.Combine(Directory.GetCurrentDirectory(), "clis", "speedtest.exe");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            speedtestPath = Path.Combine(Directory.GetCurrentDirectory(), "clis", "speedtest");
        }
        _logger.LogInformation("Using speedtest CLI at: {SpeedtestPath}", speedtestPath);

        Process? process = null;
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
            string jsonOutput = string.Empty;
            StringBuilder sb = new StringBuilder();
            process = new Process();
            process.StartInfo = startInfo;

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                sb.AppendLine(process.StandardOutput.ReadLine()!);
            }
            jsonOutput = sb.ToString();

            process.WaitForExit(TimeSpan.FromSeconds(60.0)); // Wait for a maximum of 60 seconds

            if (process.ExitCode != 0)
            {
                _logger.LogError("[CLI Error]: Speedtest CLI exited with code {ExitCode}. Error Output: {ErrorOutput}", process.ExitCode, "errorOutput");
                return Task.FromResult<SpeedtestResult?>(null);
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return Task.FromResult(JsonSerializer.Deserialize<SpeedtestResult>(jsonOutput, options));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Execution Failed]: {Message}", ex.Message);
            return Task.FromResult<SpeedtestResult?>(null);
        }
        finally
        {
            // Ensure the process is cleaned up
            if (process != null && !process.HasExited)
            {
                process.Kill();
                process.Dispose();
            }
        }
    }
}
