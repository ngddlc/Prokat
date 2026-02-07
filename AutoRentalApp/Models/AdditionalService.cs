using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("additionalservices")]
    public class AdditionalService
    {
        [Key]
        public int ServiceID { get; set; }

        [Required]
        [StringLength(50)]
        public string ServiceName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }
    }
}