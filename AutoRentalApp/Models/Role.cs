using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("roleid")]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("rolename")]
        public string RoleName { get; set; }
    }
}