namespace SpeedtestMQTT.Models;

internal class SpeedTest
{
    public double PingLatency { get; set; }

    public double DownloadSpeedMbps { get; set; }

    public double UploadSpeedMbps { get; set; }

    public string ISP { get; set; } = string.Empty;

    public string ServerName { get; set; } = string.Empty;
}
