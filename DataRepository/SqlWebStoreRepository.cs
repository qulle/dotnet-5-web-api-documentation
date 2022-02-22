using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebStore.DataContext;
using WebStore.Models;

namespace WebStore.DataRepository
{
    public class SqlWebStoreRepository : IWebStoreRepository
    {
        private readonly WebStoreContext _context;

        public SqlWebStoreRepository(WebStoreContext context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<bool> SaveChangesToDBAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public async Task CreateProductAsync(Product product)
        {
            if(product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            await _context.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            // Not used, handled by the controller
            await Task.CompletedTask;
        }

        public async Task DeleteProductAsync(Product product)
        {
            if(product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            await Task.Run(() => _context.Products.Remove(product));
        }
    }
}