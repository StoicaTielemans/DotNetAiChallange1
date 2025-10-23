using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TabTogetherApi.Entities
{
    public class Receipt
{
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Required]
        public ICollection<ReceiptItem> Items { get; set; }
        public decimal Total { get; set; }

        [MaxLength(2048)]
        public string ImageUrl { get; set; }
    }
}
