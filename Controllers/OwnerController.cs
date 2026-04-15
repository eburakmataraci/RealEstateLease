using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateLease.Data;
using RealEstateLease.Models;
using System.Linq;

namespace RealEstateLease.Controllers
{
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 Mülk sahibi dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Mülk bazlı özet bilgiler
            var propertySummaries = await _context.Properties
                .Include(p => p.Units)
                .Select(p => new OwnerDashboardPropertyVm
                {
                    PropertyId = p.Id,
                    PropertyName = p.Name,
                    UnitCount = p.Units.Count,
                    OccupiedUnitCount = p.Units.Count(u => u.IsOccupied),

                    // Bu mülkteki aktif sözleşme sayısı
                    ActiveLeaseCount = _context.Leases
                        .Count(l => l.Unit.PropertyId == p.Id && l.Status == LeaseStatus.Active),

                    // Bu mülkteki aktif sözleşmelerin toplam aylık kirası
                    MonthlyRentTotal = _context.Leases
                        .Where(l => l.Unit.PropertyId == p.Id && l.Status == LeaseStatus.Active)
                        .Sum(l => (decimal?)l.MonthlyRent) ?? 0m,

                    // Bu mülkteki ödenmemiş toplam (faturalar)
                    UnpaidTotal = _context.RentInvoices
                        .Where(inv => inv.Lease.Unit.PropertyId == p.Id &&
                                      inv.Status == InvoiceStatus.Unpaid)
                        .Sum(inv => (decimal?)(inv.Amount - inv.AmountPaid)) ?? 0m
                })
                .ToListAsync();

            var model = new OwnerDashboardViewModel
            {
                Properties = propertySummaries,
                GrandMonthlyRentTotal = propertySummaries.Sum(x => x.MonthlyRentTotal),
                GrandUnpaidTotal = propertySummaries.Sum(x => x.UnpaidTotal)
            };

            // 🔹 Son 6 ay tahsil edilen kira (grafik için)
            var today = DateTime.Today;
            var startMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

            var monthlyPaid = await _context.RentInvoices
                .Where(i => i.Status == InvoiceStatus.Paid && i.DueDate >= startMonth)
                .GroupBy(i => new { i.DueDate.Year, i.DueDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Label = g.Key.Month + "/" + g.Key.Year,
                    Total = g.Sum(x => x.AmountPaid)
                })
                .ToListAsync();

            ViewBag.ChartLabels = monthlyPaid.Select(m => m.Label).ToArray();
            ViewBag.ChartData = monthlyPaid.Select(m => m.Total).ToArray();

            return View(model);
        }
    }
}
