using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Settings
    {
        [Key] public int Id { get; set; }
        public string DeviceMac { get; set; } = "";
        public Device? Device { get; set; }

        public Watering Watering { get; set; } = new();
        public Lighting Lighting { get; set; } = new();
        public Security Security { get; set; } = new();

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Owned]
    public class Watering
    {
        public bool Manual { get; set; }
        public int SoilMin { get; set; }
        public int SoilMax { get; set; }
        public int MaxPumpSeconds { get; set; }
        public int FertHours { get; set; }
    }
    [Owned]
    public class Lighting
    {
        public bool Manual { get; set; }
        public int LuxLow { get; set; }
        public byte OnHour { get; set; }
        public byte OffHour { get; set; }
    }
    [Owned]
    public class Security
    {
        public bool Armed { get; set; }
        public string AlarmStart { get; set; } = "22:00";
        public string AlarmEnd { get; set; } = "06:00";
    }

}
