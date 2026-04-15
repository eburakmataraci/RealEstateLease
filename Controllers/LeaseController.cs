using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using RealEstateLease.Data;
using RealEstateLease.Models;
using System.IO;

namespace RealEstateLease.Controllers
{
    [Authorize] // Bu controller için login zorunlu
    public class LeaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env; // <-- yeni

        public LeaseController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // =========================
        // 1) GET Create
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var properties = await _context.Properties.ToListAsync();
            ViewBag.Properties = properties;

            return View();
        }

        // =========================
        // 2) AJAX => Units listele
        // =========================
        public async Task<IActionResult> GetUnits(int propertyId)
        {
            var units = await _context.Units
                .Where(u => u.PropertyId == propertyId) // boş/dolu hepsi
                .Select(u => new
                {
                    id = u.Id,
                    unitNumber = u.UnitNumber
                })
                .ToListAsync();

            return Json(units);
        }

        // =========================
        // 3) POST Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lease lease)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var unit = await _context.Units.FindAsync(lease.UnitId);
            if (unit == null)
                return BadRequest("Daire bulunamadı");

            // Kiracı bilgisi
            lease.TenantId = user.Id;
            lease.Tenant = user;

            // Aylık kira daireden geliyor
            lease.MonthlyRent = unit.MonthlyRent;

            // Başlangıç durumu
            lease.Status = LeaseStatus.PendingSignature;

            // Daireyi dolu işaretle
            unit.IsOccupied = true;

            // Önce sözleşmeyi kaydet ki Id'si oluşsun
            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();

            // ======= ÖDEME PLANI OLUŞTURMA =======
            var invoices = new List<RentInvoice>();

            var current = lease.StartDate;

            var endDate = lease.EndDate == DateTime.MinValue
                ? lease.StartDate.AddYears(1)
                : lease.EndDate;

            while (current < endDate)
            {
                invoices.Add(new RentInvoice
                {
                    LeaseId = lease.Id,
                    DueDate = current,
                    Amount = lease.MonthlyRent,
                    AmountPaid = 0,
                    Status = InvoiceStatus.Unpaid
                });

                current = current.AddMonths(1);
            }

            if (invoices.Any())
            {
                _context.RentInvoices.AddRange(invoices);
                await _context.SaveChangesAsync();
            }
            // ======= ÖDEME PLANI SON =======

            return RedirectToAction(nameof(MyLeases));
        }

        // =========================
        // 4) Kullanıcıya ait sözleşmeler
        // =========================
        public async Task<IActionResult> MyLeases()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var leases = await _context.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u.Property)
                .Include(l => l.Invoices)
                .Where(l => l.TenantId == user.Id)
                .ToListAsync();

            return View(leases);
        }

        // =========================
        // 5) Detay sayfası
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u.Property)
                .Include(l => l.Invoices)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null)
                return NotFound();

            // --- Bu sözleşmeye ait yüklenmiş dosyaları listele ---
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "leases");
            Directory.CreateDirectory(uploadsRoot);

            var files = Directory.GetFiles(uploadsRoot, $"lease_{id}_*")
                                 .Select(Path.GetFileName)
                                 .ToList();

            ViewBag.LeaseFiles = files;

            return View(lease);
        }

        // =========================
        // 6) Sözleşme İmzala (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sign(int id)
        {
            var lease = await _context.Leases.FindAsync(id);
            if (lease == null)
                return NotFound();

            lease.Status = LeaseStatus.Active;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sözleşme başarıyla imzalandı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // =========================
        // 7) ÖDEME PLANI OLUŞTUR
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInvoices(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Invoices)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lease == null)
                return NotFound();

            if (lease.Invoices != null && lease.Invoices.Any())
            {
                TempData["Info"] = "Bu sözleşme için zaten bir ödeme planı oluşturulmuş.";
                return RedirectToAction("Details", new { id });
            }

            var date = lease.StartDate.Date;

            if (lease.EndDate < lease.StartDate)
                lease.EndDate = lease.StartDate;

            while (date <= lease.EndDate.Date)
            {
                var invoice = new RentInvoice
                {
                    LeaseId = lease.Id,
                    DueDate = date,
                    Amount = lease.MonthlyRent,
                    AmountPaid = 0m,
                    Status = InvoiceStatus.Unpaid
                };

                _context.RentInvoices.Add(invoice);
                date = date.AddMonths(1);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ödeme planı başarıyla yeniden oluşturuldu.";
            return RedirectToAction("Details", new { id });
        }

        // =========================
        // 8) Belge Yükle
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int id, IFormFile file)
        {
            var lease = await _context.Leases.FindAsync(id);
            if (lease == null)
                return NotFound();

            if (file == null || file.Length == 0)
            {
                TempData["Info"] = "Yüklenecek bir dosya seçmediniz.";
                return RedirectToAction("Details", new { id });
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "leases");
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".dat";

            var fileName = $"lease_{id}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            TempData["Success"] = "Belge başarıyla yüklendi.";
            return RedirectToAction("Details", new { id });
        }
    }
}
