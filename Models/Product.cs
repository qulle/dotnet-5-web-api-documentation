using System.ComponentModel.DataAnnotations;

namespace WebStore.Models 
{
    public class Product {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public double Price { get; set; }

        [Required]
        [MaxLength(36)]
        public string VendorGuid { get; set; }
    }
}