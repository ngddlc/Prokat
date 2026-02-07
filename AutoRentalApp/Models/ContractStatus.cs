using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("contractstatuses")]
    public class ContractStatus
    {
        [Key]
        [Column("contractstatusid")]
        public int ContractStatusID { get; set; }

        [Required]
        [Column("statusname")]
        public string StatusName { get; set; }
    }
}