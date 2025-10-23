using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TabTogetherApi.Entities
{
    public class ReceiptItem
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty;

        public int? Amount { get; set; }

        public decimal? Price { get; set; }

        public double Confidence { get; set; }

        // Foreign key to Receipt (required)
        [Required]
        public Guid ReceiptId { get; set; }
        public Receipt? Receipt { get; set; }

        // FriendId is required (non-nullable)
        public Guid FriendId { get; set; }
        public Friend? Friend { get; set; }
    }
}