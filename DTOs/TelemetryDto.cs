namespace api.DTOs
{
    public class TelemetryDto
    {
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Soil { get; set; }
        public float Lux { get; set; }
        public float Level { get; set; }
        public bool Tamper { get; set; }
    }
}
