using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisticsSystemManagementApi.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PickupStreet { get; set; }

        [Required]
        [MaxLength(50)]
        public string PickupCity { get; set; }

        [Required]
        [MaxLength(20)]
        public string PickupPostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeliveryStreet { get; set; }

        [Required]
        [MaxLength(50)]
        public string DeliveryCity { get; set; }

        [Required]
        [MaxLength(20)]
        public string DeliveryPostalCode { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PackageWeight { get; set; }

        public string? OrderDescription { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public int OrderStatusId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}