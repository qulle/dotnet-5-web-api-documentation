using AutoMapper;
using WebStore.DtoModels.V1;
using WebStore.Models;

namespace WebStore.DtoProfiles.V1
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