using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebStore.DataRepository;
using WebStore.DtoModels.V1;
using WebStore.Models;
using Serilog;

namespace WebStore.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/products")]
    [Route("api/v{version:apiVersion}/products")]
    [Produces("application/json")]
    [ApiController]
    public class ProductsController : ControllerBase 
    {
        private readonly IWebStoreRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProductsController(IWebStoreRepository repository, IMapper mapper, ILogger logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts()
        {
            var products = await _repository.GetProductsAsync();

            return Ok(_mapper.Map<IEnumerable<ProductReadDto>>(products));
        }

        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductReadDto>> GetProductById(int id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            
            if(product != null)
            {
                return Ok(_mapper.Map<ProductReadDto>(product));
            }

            return NotFound();
        }

        [HttpPost]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<ProductReadDto>> CreateProduct([FromBody] ProductCreateDto productCreateDto, [FromRoute] ApiVersion apiVersion)
        {
            var productModel = _mapper.Map<Product>(productCreateDto);

            await _repository.CreateProductAsync(productModel);
            await _repository.SaveChangesToDBAsync();

            var productReadDto = _mapper.Map<ProductReadDto>(productModel);

            return CreatedAtAction(nameof(GetProductById), new {Id = productReadDto.Id, Version = apiVersion.ToString()}, productReadDto);
        }

        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProduct(int id, ProductUpdateDto productUpdateDto)
        {
            var productModelFromRepo = await _repository.GetProductByIdAsync(id);
            if(productModelFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(productUpdateDto, productModelFromRepo);

            await _repository.UpdateProductAsync(productModelFromRepo);
            await _repository.SaveChangesToDBAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PartialProductUpdate(int id, JsonPatchDocument<ProductUpdateDto> pathDocument) 
        {
            var productModelFromRepo = await _repository.GetProductByIdAsync(id);
            if(productModelFromRepo == null)
            {
                return NotFound();
            }

            var productToPatch = _mapper.Map<ProductUpdateDto>(productModelFromRepo);
            pathDocument.ApplyTo(productToPatch, ModelState);

            if(!TryValidateModel(productToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(productToPatch, productModelFromRepo);

            await _repository.UpdateProductAsync(productModelFromRepo);
            await _repository.SaveChangesToDBAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var productModelFromRepo = await _repository.GetProductByIdAsync(id);
            if(productModelFromRepo == null)
            {
                return NotFound();
            }

            await _repository.DeleteProductAsync(productModelFromRepo);
            await _repository.SaveChangesToDBAsync();

            return NoContent();
        }
    }
}