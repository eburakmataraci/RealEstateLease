using System.Collections.Generic;

namespace RealEstateLease.Models
{
    /// <summary>
    /// Her mülk için satır bazlı istatistik
    /// </summary>
    public class OwnerDashboardPropertyVm
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;

        public int UnitCount { get; set; }              // Toplam daire
        public int OccupiedUnitCount { get; set; }      // Dolu daire
        public int ActiveLeaseCount { get; set; }       // Aktif sözleşme

        public decimal MonthlyRentTotal { get; set; }   // Bu mülkteki aktif sözleşmelerin toplam aylık kirası
        public decimal UnpaidTotal { get; set; }        // Bu mülkteki ödenmemiş tutar (faturalar)
    }

    /// <summary>
    /// Dashboard'ın ana view modeli
    /// </summary>
    public class OwnerDashboardViewModel
    {
        public List<OwnerDashboardPropertyVm> Properties { get; set; } = new();

        // Tüm mülkler için özet
        public decimal GrandMonthlyRentTotal { get; set; }
        public decimal GrandUnpaidTotal { get; set; }
    }
}
