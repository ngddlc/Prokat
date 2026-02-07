using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("carinspections")]
    public class CarInspection
    {
        [Key]
        [Column("inspectionid")]
        public int InspectionID { get; set; }

        [Required]
        [Column("contractid")]
        public int ContractID { get; set; }

        [Required]
        [Column("inspectiontype")]
        public string InspectionType { get; set; }

        [Required]
        [Column("inspectiondate")]
        public DateTime InspectionDate { get; set; }

        [Required]
        [Column("notes")]
        public string Notes { get; set; }

        [Required]
        [Column("damagecost")]
        public decimal DamageCost { get; set; }

        [ForeignKey("ContractID")]
        public virtual RentalContract RentalContract { get; set; }
    }
}