using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;

namespace RealEstateLease.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================
        // 1) Ödemelerim (tüm faturalar)
        // ================
        public async Task<IActionResult> MyInvoices()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var invoices = await _context.RentInvoices
                .Include(i => i.Lease)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u.Property)
                .Where(i => i.Lease.TenantId == user.Id)
                .OrderByDescending(i => i.DueDate)
                .ToListAsync();

            return View(invoices);
        }

        // ================
        // 2) Tek fatura ödeme ekranı (GET)
        // ================
        public async Task<IActionResult> Pay(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var invoice = await _context.RentInvoices
                .Include(i => i.Lease)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(i => i.Id == id && i.Lease.TenantId == user.Id);

            if (invoice == null)
                return NotFound();

            if (invoice.Status == InvoiceStatus.Paid)
            {
                TempData["Info"] = "Bu fatura zaten ödenmiş.";
                return RedirectToAction("Details", "Lease", new { id = invoice.LeaseId });
            }

            return View(invoice);
        }

        // ================
        // 3) Ödemeyi tamamlama (POST)
        // ================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var invoice = await _context.RentInvoices
                .Include(i => i.Lease)
                .FirstOrDefaultAsync(i => i.Id == id && i.Lease.TenantId == user.Id);

            if (invoice == null)
                return NotFound();

            if (invoice.Status == InvoiceStatus.Paid)
            {
                TempData["Info"] = "Bu fatura zaten ödenmiş.";
                return RedirectToAction("Details", "Lease", new { id = invoice.LeaseId });
            }

            // Burada normalde kart bilgisi, banka entegrasyonu vs olur.
            // Şimdilik "tam ödendi" kabul ediyoruz.
            invoice.AmountPaid = invoice.Amount;
            invoice.Status = InvoiceStatus.Paid;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ödeme başarıyla işlendi.";
            return RedirectToAction("Details", "Lease", new { id = invoice.LeaseId });
        }
    }
}
