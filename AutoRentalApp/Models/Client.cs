using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("clients")]
    public class Client
    {
        [Key]
        [Column("clientid")]
        public int ClientID { get; set; }

        [Required]
        [Column("passportnumber")]
        public string PassportNumber { get; set; }

        [Required]
        [Column("driverlicensenumber")]
        public string DriverLicenseNumber { get; set; }

        [Required]
        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("userid")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}