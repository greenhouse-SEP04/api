namespace api.DTOs
{
    public class SettingsDto
    {
        public bool WateringManual { get; set; }
        public int SoilMin { get; set; }
        public int SoilMax { get; set; }
        public int FertHours { get; set; }
        public bool LightingManual { get; set; }
        public int LuxLow { get; set; }
        public byte OnHour { get; set; }
        public byte OffHour { get; set; }
    }
}
