using Microsoft.AspNetCore.Identity;

namespace API;

public class Role : IdentityRole<int>
{
    public ICollection<UserRole> UserRoles { get; set; }
}
