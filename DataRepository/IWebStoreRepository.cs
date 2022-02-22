using System.Collections.Generic;
using System.Threading.Tasks;
using WebStore.Models;

namespace WebStore.DataRepository
{
    public interface IWebStoreRepository
    {
        Task<bool> SaveChangesToDBAsync();
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Product product);
    }
}