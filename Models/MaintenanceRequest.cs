using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateLease.Models
{
    public enum MaintenanceStatus
    {
        New = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class MaintenanceRequest
    {
        public int Id { get; set; }

        [Required]
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        public int? LeaseId { get; set; }
        public Lease? Lease { get; set; }

        [Required]
        public string TenantId { get; set; } = string.Empty;
        public ApplicationUser Tenant { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? TenantNote { get; set; }
        public string? OwnerNote { get; set; }

        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
