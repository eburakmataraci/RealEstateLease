using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;

namespace RealEstateLease.Controllers
{
    [Authorize]
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Mülk listesi
        public async Task<IActionResult> Index()
        {
            var list = await _context.Properties.ToListAsync();
            return View(list);
        }

        // GET: Yeni mülk
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Yeni mülk
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (!ModelState.IsValid)
                return View(property);

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
