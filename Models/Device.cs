using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Device
    {
        [Key] public string Mac { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? OwnerId { get; set; }
        public User? Owner { get; set; }
        public Settings? CurrentSettings { get; set; }
        public ICollection<Telemetry>? Telemetries { get; set; }
    }
}
