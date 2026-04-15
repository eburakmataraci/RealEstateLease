using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;

namespace RealEstateLease.Controllers
{
    [Authorize]
    public class UnitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UnitController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // 1) Belirli mülke ait daireler
        // =========================
        public async Task<IActionResult> Index(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
                return NotFound();

            var units = await _context.Units
                .Where(u => u.PropertyId == propertyId)
                .ToListAsync();

            // Foto dosyası varsa PhotoFileName doldur
            var folderPath = Path.Combine(_env.WebRootPath, "images", "units");
            Directory.CreateDirectory(folderPath);

            foreach (var u in units)
            {
                // unit_{Id}.jpg / .png / .jpeg arayalım
                string[] possibleExt = [".jpg", ".jpeg", ".png", ".webp"];

                string? found = null;
                foreach (var ext in possibleExt)
                {
                    var filePath = Path.Combine(folderPath, $"unit_{u.Id}{ext}");
                    if (System.IO.File.Exists(filePath))
                    {
                        found = $"unit_{u.Id}{ext}";
                        break;
                    }
                }

                u.PhotoFileName = found;
            }

            ViewBag.Property = property;
            return View(units);
        }

        // =========================
        // 2) Yeni daire formu
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
                return NotFound();

            var unit = new Unit
            {
                PropertyId = propertyId,
                IsOccupied = false
            };

            ViewBag.Property = property;
            return View(unit);
        }

        // =========================
        // 3) Yeni daire kaydı + fotoğraf
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Unit unit, IFormFile? photo)
        {
            var property = await _context.Properties.FindAsync(unit.PropertyId);
            if (property == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Property = property;
                return View(unit);
            }

            _context.Units.Add(unit);
            await _context.SaveChangesAsync(); // Id oluşsun

            // Fotoğraf yüklendiyse kaydet
            if (photo != null && photo.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "units");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(photo.FileName);
                if (string.IsNullOrWhiteSpace(ext))
                    ext = ".jpg";

                var fileName = $"unit_{unit.Id}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await photo.CopyToAsync(stream);
                }
            }

            return RedirectToAction("Index", new { propertyId = unit.PropertyId });
        }

        // =========================
        // 4) Detay (opsiyonel, sende varsa)
        // =========================
        public async Task<IActionResult> Details(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Property)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
                return NotFound();

            // Foto dosyasını kontrol et
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "units");
            string[] possibleExt = [".jpg", ".jpeg", ".png", ".webp"];
            foreach (var ext in possibleExt)
            {
                var filePath = Path.Combine(uploadsFolder, $"unit_{unit.Id}{ext}");
                if (System.IO.File.Exists(filePath))
                {
                    unit.PhotoFileName = $"unit_{unit.Id}{ext}";
                    break;
                }
            }

            return View(unit);
        }
    }
}
