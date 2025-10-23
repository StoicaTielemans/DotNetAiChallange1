using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TabTogetherApi.Entities
{
    public class Friend
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
       public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]

         public string FirstName { get; set; }

        [MaxLength(100)]
         public string LastName { get; set; }

        [MaxLength(32)]
        public string PhoneNumber { get; set; }

    }
}
