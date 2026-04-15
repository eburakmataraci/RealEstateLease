using System.ComponentModel.DataAnnotations;

namespace RealEstateLease.Models
{
    /// <summary>
    /// Bakım talebi oluşturma ekranında kullanılacak ViewModel
    /// </summary>
    public class MaintenanceCreateVm
    {
        [Required]
        [Display(Name = "Sözleşme / Daire")]
        public int LeaseId { get; set; }

        // Drop-down için kiracının sözleşmeleri
        public List<Lease>? Leases { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Ek Notunuz")]
        public string? TenantNote { get; set; }
    }
}
