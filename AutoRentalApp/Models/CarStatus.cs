using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("carstatuses")]
    public class CarStatus
    {
        [Key]
        [Column("carstatusid")]
        public int CarStatusID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("statusname")]
        public string StatusName { get; set; }
    }
}