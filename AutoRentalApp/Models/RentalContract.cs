using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("rentalcontracts")]
    public class RentalContract
    {
        [Key]
        [Column("contractid")]
        public int ContractID { get; set; }

        [Required]
        [Column("startdatetime")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Column("plannedenddatetime")]
        public DateTime PlannedEndDateTime { get; set; }

        [Column("actualenddatetime")]
        public DateTime? ActualEndDateTime { get; set; }

        [Required]
        [Column("totalamount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column("clientid")]
        public int ClientID { get; set; }

        [Required]
        [Column("carid")]
        public int CarID { get; set; }

        [Required]
        [Column("managerid")]
        public int ManagerID { get; set; }

        [Required]
        [Column("contractstatusid")]
        public int ContractStatusID { get; set; }

        [ForeignKey("ClientID")]
        public virtual Client Client { get; set; }

        [ForeignKey("CarID")]
        public virtual Car Car { get; set; }

        [ForeignKey("ManagerID")]
        public virtual Employee Manager { get; set; }

        [ForeignKey("ContractStatusID")]
        public virtual ContractStatus ContractStatus { get; set; }

        [NotMapped]
        public string ContractNumber => $"ДОГ-{ContractID:D6}";
    }
}