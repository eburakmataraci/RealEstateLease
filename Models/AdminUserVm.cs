namespace RealEstateLease.Models
{
    public class AdminUserVm
    {
        public string Id       { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string? UserName { get; set; }

        // Virgülle ayrılmış rol listesi (Admin, Owner, Tenant)
        public string Roles    { get; set; } = string.Empty;
    }
}
