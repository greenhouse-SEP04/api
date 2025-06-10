using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Device>? Devices { get; set; }
    }
}
