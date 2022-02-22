using System.ComponentModel.DataAnnotations;

namespace WebStore.DtoModels.V1
{
    public class ProductCreateDto 
    {
        [Required]
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