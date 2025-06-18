using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class User : IdentityUser
    {
        public bool IsFirstLogin { get; set; } = true;

        public ICollection<Device>? Devices { get; set; }
    }
}
