using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateLease.Models
{
    public class Unit
    {
        public int Id { get; set; }

        // Hangi mülke ait
        [Required]
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        // Daire numarası (örn: Daire 5)
        [Required]
        [StringLength(50)]
        public string UnitNumber { get; set; } = string.Empty;

        // Kaçıncı kat
        public int Floor { get; set; }

        // Aylık kira
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRent { get; set; }

        // Daire dolu mu
        public bool IsOccupied { get; set; }

        // Daire fotoğrafının dosya adı (wwwroot/images/units altında)
        [StringLength(260)]
        public string? PhotoFileName { get; set; }

        // İlişkiler
        public ICollection<Lease> Leases { get; set; } = new List<Lease>();
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    }
}
