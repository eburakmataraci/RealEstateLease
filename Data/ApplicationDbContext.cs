using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Models;

namespace RealEstateLease.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<Unit> Units { get; set; } = null!;
        public DbSet<Lease> Leases { get; set; } = null!;
        public DbSet<RentInvoice> RentInvoices { get; set; } = null!;

        // Bakım talepleri
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // İlişki ayarların varsa burada kalabilir
        }
    }
}
