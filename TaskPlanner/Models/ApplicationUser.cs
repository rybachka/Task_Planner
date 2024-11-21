using Microsoft.AspNetCore.Identity;

namespace TaskPlanner.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string ProfilePicturePath { get; set; } = "/uploads/default-profile.png";

    }
}
