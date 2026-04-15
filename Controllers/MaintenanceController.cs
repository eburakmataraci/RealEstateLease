using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;

namespace RealEstateLease.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // Kiracının kendi talepleri
        // =========================
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var list = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                    .ThenInclude(u => u.Property)
                .Where(m => m.TenantId == user.Id)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(list);
        }

        // =========================
        // Ev sahibi bakım listesi
        // =========================
        [Authorize(Roles = AppRoles.Owner)]
        public async Task<IActionResult> OwnerList(
            int? propertyId,
            int? unitId,
            RealEstateLease.Models.MaintenanceStatus? status)
        {
            var owner = await _userManager.GetUserAsync(User);
            if (owner == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var query = _context.MaintenanceRequests
                .Include(m => m.Unit)
                    .ThenInclude(u => u.Property)
                .Include(m => m.Tenant)
                .AsQueryable();

            if (propertyId.HasValue)
                query = query.Where(m => m.Unit.PropertyId == propertyId.Value);

            if (unitId.HasValue)
                query = query.Where(m => m.UnitId == unitId.Value);

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            var list = await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(list);
        }

        // =========================
        // YENİ TALEP – GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var leases = await _context.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u.Property)
                .Where(l => l.TenantId == user.Id)
                .ToListAsync();

            ViewBag.Leases = leases;
            return View();
        }

        // =========================
        // YENİ TALEP – POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int leaseId,
            string title,
            string? description,
            string? tenantNote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            var lease = await _context.Leases
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.Id == leaseId && l.TenantId == user.Id);

            if (lease == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz sözleşme.");
                return View();
            }

            var request = new MaintenanceRequest
            {
                LeaseId = lease.Id,
                UnitId = lease.UnitId,
                TenantId = user.Id,
                Title = title,
                Description = description,
                TenantNote = tenantNote,
                Status = MaintenanceStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bakım talebiniz oluşturuldu.";
            return RedirectToAction(nameof(MyRequests));
        }

        // =========================
        // Detay
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var req = await _context.MaintenanceRequests
                .Include(m => m.Unit)
                    .ThenInclude(u => u.Property)
                .Include(m => m.Tenant)
                .Include(m => m.Lease)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (req == null)
                return NotFound();

            return View(req);
        }

        // =========================
        // Durum Güncelle (Ev Sahibi)
        // =========================
        [HttpPost]
        [Authorize(Roles = AppRoles.Owner)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            RealEstateLease.Models.MaintenanceStatus status,
            string? ownerNote)
        {
            var req = await _context.MaintenanceRequests.FindAsync(id);
            if (req == null)
                return NotFound();

            req.Status = status;
            if (!string.IsNullOrWhiteSpace(ownerNote))
                req.OwnerNote = ownerNote;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Talep güncellendi.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
