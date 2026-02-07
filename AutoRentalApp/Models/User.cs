using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("userid")]
        public int UserID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Column("firstname")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Column("lastname")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Column("login")]
        public string Login { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 6)]
        [Column("passwordhash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("roleid")]
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }

        public string FullName => $"{LastName} {FirstName}";
    }
}