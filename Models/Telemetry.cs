using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Telemetry
    {
        [Key] public int Id { get; set; }
        public string DeviceMac { get; set; } = string.Empty;
        public Device? Device { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Soil { get; set; }
        public float Lux { get; set; }
        public float Level { get; set; }
        public bool Tamper { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
