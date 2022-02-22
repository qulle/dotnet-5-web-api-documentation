using Microsoft.EntityFrameworkCore;
using WebStore.Models;

public static class ModelBuilderExtensions
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product {Id = 1, Name = "Vortex Race 3", Quantity = 4, Price = 150, VendorGuid = "d406189b-01a1-404b-8147-cf9a81e1c283"},
            new Product {Id = 2, Name = "Varmilo VA88M", Quantity = 0, Price = 180, VendorGuid = "29077cea-6bdc-4dc4-b441-b325fc9a2797"},
            new Product {Id = 3, Name = "Ducky One 2 Mini", Quantity = 1, Price = 139, VendorGuid = "8b43de00-475a-4db2-be34-71a3d1ba40b1"},
            new Product {Id = 4, Name = "Keychron K8", Quantity = 2, Price = 89, VendorGuid = "f534302c-7c1b-4954-9cb9-9576fd1c7dd8"}
        );
    }
}