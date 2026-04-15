using Microsoft.AspNetCore.Identity;

namespace RealEstateLease.Models
{
    public enum UserType
    {
        Admin = 0,
        Owner = 1,   // Ev sahibi
        Tenant = 2   // Kiracı
    }

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserType UserType { get; set; } = UserType.Tenant;
    }
}
