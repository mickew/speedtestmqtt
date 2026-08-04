using System.ComponentModel.DataAnnotations;

namespace SpeedtestMQTT.Models;

internal class MqttSettings
{
    public const string Section = "MQTT";

    [Required]
    public string? Server { get; set; }

    [Required]
    public int Port { get; set; } = 1883;

    [Required]
    public string? ClientId { get; set; }

    [Required]
    public string? Topic { get; set; }
}
