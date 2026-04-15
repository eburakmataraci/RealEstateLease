using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateLease.Models
{
    public enum LeaseStatus
    {
        PendingSignature = 0,
        Active = 1,
        Terminated = 2
    }

    public enum InvoiceStatus
    {
        Unpaid = 0,
        PartiallyPaid = 1,
        Paid = 2
    }

    public class Lease
    {
        public int Id { get; set; }

        // Daire ilişkisi
        [Required]
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        // Kiracı ilişkisi
        [Required]
        public string TenantId { get; set; } = string.Empty;
        public ApplicationUser Tenant { get; set; } = null!;

        // Tarihler
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Aylık kira
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRent { get; set; }

        public LeaseStatus Status { get; set; } = LeaseStatus.PendingSignature;

        // İlgili faturalar
        public ICollection<RentInvoice> Invoices { get; set; } = new List<RentInvoice>();
    }

    public class RentInvoice
    {
        public int Id { get; set; }

        [Required]
        public int LeaseId { get; set; }
        public Lease Lease { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    }
}
