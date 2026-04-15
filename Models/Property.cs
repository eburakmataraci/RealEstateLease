using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateLease.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        // Bu mülke bağlı daireler
        public ICollection<Unit> Units { get; set; } = new List<Unit>();

        // FOTOĞRAF – veritabanına gitmesin diye NotMapped
        [NotMapped]
        public string? PhotoFileName { get; set; }
    }
}
