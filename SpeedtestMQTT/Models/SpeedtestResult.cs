using System.Text.Json.Serialization;

namespace SpeedtestMQTT.Models;


internal record SpeedtestResult(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("ping")] PingInfo Ping,
    [property: JsonPropertyName("download")] TransferInfo Download,
    [property: JsonPropertyName("upload")] TransferInfo Upload,
    [property: JsonPropertyName("isp")] string Isp,
    [property: JsonPropertyName("interface")] InterfaceInfo Interface,
    [property: JsonPropertyName("server")] ServerInfo Server,
    [property: JsonPropertyName("result")] ResultInfo Result
);

internal record InterfaceInfo(
    [property: JsonPropertyName("internalIp")] string InternalIp,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("macAddr")] string MacAddr,
    [property: JsonPropertyName("isVpn")] bool IsVpn,
    [property: JsonPropertyName("externalIp")] string ExternalIp
);

internal record PingInfo(
    [property: JsonPropertyName("jitter")] double Jitter,
    [property: JsonPropertyName("latency")] double Latency,
    [property: JsonPropertyName("low")] double Low,
    [property: JsonPropertyName("high")] double High
);

internal record LatencyInfo(
    [property: JsonPropertyName("iqm")] double Iqm,
    [property: JsonPropertyName("low")] double Low,
    [property: JsonPropertyName("high")] double High,
    [property: JsonPropertyName("jitter")] double Jitter
);

internal record TransferInfo(
    [property: JsonPropertyName("bandwidth")] int Bandwidth,
    [property: JsonPropertyName("bytes")] int Bytes,
    [property: JsonPropertyName("elapsed")] int Elapsed,
    [property: JsonPropertyName("latency")] LatencyInfo Latency
);

internal static class TransferInfoExtensions
{
    public static double BanndwithToMbps<T>(this T source) where T : TransferInfo
    {
        return source.Bandwidth * 8 / 1_000_000.0;
    }
}

internal record ServerInfo(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("host")] string Host,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("ip")] string Ip
);

internal record ResultInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("persisted")] bool Persisted
);
