using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoRentalApp.Models
{
    [Table("cars")]
    public class Car
    {
        [Key]
        [Column("carid")]
        public int CarID { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^[А-Я]{1}\d{3}[А-Я]{2}\s\d{2,3}$")]
        [Column("platenumber")]
        public string PlateNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Column("brand")]
        public string Brand { get; set; }

        [Required]
        [StringLength(50)]
        [Column("model")]
        public string Model { get; set; }

        [Required]
        [Range(1990, 2030)]
        [Column("year")]
        public int Year { get; set; }

        [Required]
        [StringLength(30)]
        [Column("color")]
        public string Color { get; set; }

        [Required]
        [StringLength(30)]
        [Column("bodytype")]
        public string BodyType { get; set; }

        [Required]
        [StringLength(20)]
        [Column("enginetype")]
        public string EngineType { get; set; }

        [Required]
        [Range(0.5, 10.0)]
        [Column("enginevolume")]
        public decimal EngineVolume { get; set; }

        [Required]
        [StringLength(20)]
        [Column("transmissiontype")]
        public string TransmissionType { get; set; }

        [Required]
        [StringLength(20)]
        [Column("carclass")]
        public string CarClass { get; set; }

        [Required]
        [Range(100, 50000)]
        [Column("dailyprice")]
        public decimal DailyPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        [Column("mileage")]
        public int Mileage { get; set; }

        [Column("lastservicedate")]
        public DateTime? LastServiceDate { get; set; }

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [Column("vin")]
        public string VIN { get; set; }

        [Required]
        [Column("carstatusid")] 
        public int CarStatusID { get; set; }

        [ForeignKey("CarStatusID")]
        public virtual CarStatus CarStatus { get; set; }

        [NotMapped]
        public string DisplayName => $"{Brand} {Model} ({PlateNumber})";
    }
}