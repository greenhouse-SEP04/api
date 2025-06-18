using System.Text.Json.Serialization;

namespace api.DTOs
{
    public sealed class TelemetryDto
    {
        [JsonPropertyName("ts")] public string Ts { get; set; } = "";
        [JsonPropertyName("cfgRev")] public string CfgRev { get; set; } = "";

        [JsonPropertyName("temp")] public short Temperature { get; set; }
        [JsonPropertyName("hum")] public short Humidity { get; set; }
        [JsonPropertyName("soil")] public short Soil { get; set; }
        [JsonPropertyName("lux")] public ushort Lux { get; set; }
        [JsonPropertyName("lvl")] public ushort Level { get; set; }

        [JsonPropertyName("motion")] public bool Motion { get; set; }
        [JsonPropertyName("tamper")] public bool Tamper { get; set; }

        [JsonPropertyName("accel")] public short[] Accel { get; set; } = new short[3];
    }
}
