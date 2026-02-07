using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("contractservices")]
    public class ContractService
    {
        [Key]
        public int ContractServiceID { get; set; }

        [Required]
        public int ContractID { get; set; }

        [Required]
        public int ServiceID { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal ServicePrice { get; set; }

        public virtual RentalContract Contract { get; set; }
        public virtual AdditionalService Service { get; set; }
    }
}