using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;

namespace RealEstateLease.Controllers
{
    [Authorize]
    public class RentInvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RentInvoiceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Faturayı "Ödendi" yap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var invoice = await _context.RentInvoices
                .Include(i => i.Lease)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Güvenlik: sadece kendi sözleşmesinin faturasını güncellesin
            if (invoice.Lease.TenantId != user.Id)
                return Forbid();

            invoice.AmountPaid = invoice.Amount;
            invoice.Status = InvoiceStatus.Paid;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Lease", new { id = invoice.LeaseId });
        }
    }
}
