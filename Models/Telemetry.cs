using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Telemetry
    {
        [Key] public int Id { get; set; }
        public string DeviceMac { get; set; } = "";
        public Device? Device { get; set; }

        public short Temperature { get; set; }
        public short Humidity { get; set; }
        public short Soil { get; set; }
        public ushort Lux { get; set; }
        public ushort Level { get; set; }

        public bool Motion { get; set; }
        public bool Tamper { get; set; }
        public short AccelX { get; set; }
        public short AccelY { get; set; }
        public short AccelZ { get; set; }

        public string CfgRev { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
