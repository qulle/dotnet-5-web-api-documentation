using AutoMapper;
using WebStore.DtoModels.V2;
using WebStore.Models;

namespace WebStore.DtoProfiles.V2
{
    public class ProductsProfile : Profile
    {
        public ProductsProfile()
        {
            // CreateMap<Source, Target>()
            CreateMap<Product, ProductReadDto>();
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();
            CreateMap<Product, ProductUpdateDto>();
        }
    }
}