using System.Collections.Generic;
using System.Threading.Tasks;
using WebStore.Models;

namespace WebStore.DataRepository
{
    public class MockWebStoreRepository : IWebStoreRepository
    {
        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await Task.FromResult(new Product 
            {
                Id = id, 
                Name = "Mock Vortex Race 3", 
                Quantity = 4, 
                Price = 150,
                VendorGuid = "428f3089-616b-4f88-99be-0d34abc26701"
            });
        }
        
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await Task.FromResult(new List<Product>
            {
                new Product {Id = 0, Name = "Mock Vortex Race 3", Quantity = 4, Price = 150, VendorGuid = "d406189b-01a1-404b-8147-cf9a81e1c283"},
                new Product {Id = 1, Name = "Mock Varmilo VA88M", Quantity = 0, Price = 180, VendorGuid = "29077cea-6bdc-4dc4-b441-b325fc9a2797"},
                new Product {Id = 2, Name = "Mock Ducky One 2 Mini", Quantity = 1, Price = 139, VendorGuid = "8b43de00-475a-4db2-be34-71a3d1ba40b1"},
                new Product {Id = 3, Name = "Mock Keychron K8", Quantity = 2, Price = 89, VendorGuid = "f534302c-7c1b-4954-9cb9-9576fd1c7dd8"}
            });
        }

        public Task<bool> SaveChangesToDBAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task CreateProductAsync(Product product)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateProductAsync(Product product)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteProductAsync(Product product)
        {
            throw new System.NotImplementedException();
        }
    }
}