using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("employees")]
    public class Employee
    {
        [Key]
        [Column("employeeid")]
        public int EmployeeID { get; set; }

        [Required]
        [Column("position")]
        public string Position { get; set; }

        [Required]
        [Column("hiredate")]
        public DateTime HireDate { get; set; }

        [Required]
        [Column("userid")]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}