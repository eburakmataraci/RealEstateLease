using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Models;

namespace RealEstateLease.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope   = services.CreateScope();
            var provider      = scope.ServiceProvider;

            var context     = provider.GetRequiredService<ApplicationDbContext>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1) Migration yerine doğrudan tablo oluştur
            await context.Database.EnsureCreatedAsync();

            // 2) Rolleri oluştur
            string[] roles = { AppRoles.Admin, AppRoles.Owner, AppRoles.Tenant };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3) Varsayılan admin
            var adminEmail = "admin@kira.local";
            var adminUser  = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
            {
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
            }

            // 4) Eğer zaten sözleşme varsa seed kira/fatura ekleme
            if (await context.Leases.AnyAsync())
                return;

            var tenant = await userManager.Users.FirstOrDefaultAsync();
            if (tenant == null)
                return;

            var property = new Property
            {
                Name    = "Örnek Apartman",
                Address = "İstanbul, Örnek Mahallesi 123"
            };

            var unit = new Unit
            {
                Property    = property,
                UnitNumber  = "Daire 5",
                Floor       = 1,
                MonthlyRent = 15000m,
                IsOccupied  = true
            };

            var lease = new Lease
            {
                Unit        = unit,
                Tenant      = tenant,
                TenantId    = tenant.Id,
                StartDate   = DateTime.Today,
                EndDate     = DateTime.Today.AddYears(1),
                MonthlyRent = unit.MonthlyRent,
                Status      = LeaseStatus.PendingSignature
            };

            var invoice = new RentInvoice
            {
                Lease      = lease,
                DueDate    = DateTime.Today.AddDays(7),
                Amount     = lease.MonthlyRent,
                AmountPaid = 0,
                Status     = InvoiceStatus.Unpaid
            };

            context.Properties.Add(property);
            context.Units.Add(unit);
            context.Leases.Add(lease);
            context.RentInvoices.Add(invoice);

            await context.SaveChangesAsync();
        }
    }
}
