namespace api.DTOs
{
    public sealed class WateringDto
    {
        public bool Manual { get; set; }
        public int SoilMin { get; set; }
        public int SoilMax { get; set; }
        public int MaxPumpSeconds { get; set; }
        public int FertHours { get; set; }
    }
    public sealed class LightingDto
    {
        public bool Manual { get; set; }
        public int LuxLow { get; set; }
        public byte OnHour { get; set; }
        public byte OffHour { get; set; }
    }
    public sealed class AlarmWindowDto
    {
        public string Start { get; set; } = "22:00";
        public string End { get; set; } = "06:00";
    }
    public sealed class SecurityDto
    {
        public bool Armed { get; set; }
        public AlarmWindowDto AlarmWindow { get; set; } = new();
    }

    public sealed class SettingsDto      // payload coming *from* web-client
    {
        public WateringDto Watering { get; set; } = new();
        public LightingDto Lighting { get; set; } = new();
        public SecurityDto Security { get; set; } = new();
    }

}
