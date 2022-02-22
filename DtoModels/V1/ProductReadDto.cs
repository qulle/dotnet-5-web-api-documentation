namespace WebStore.DtoModels.V1
{
    public class ProductReadDto 
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public int Quantity { get; set; }
        
        public double Price { get; set; }
    }
}