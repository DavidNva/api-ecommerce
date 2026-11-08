using api_ecommerce.Models;
using api_ecommerce.Models.Dtos;
using api_ecommerce.Repository;
using api_ecommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace api_ecommerce.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public ProductsController(
        IProductRepository productRepository, 
        IMapper mapper, 
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProducts()
    {
        var products = _productRepository.GetProducts();
        var productsDto = _mapper.Map<List<ProductDto>>(products);

        return Ok(productsDto);
    }

    [HttpGet("{productId:int}", Name = "GetProduct")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProduct(int productId)
    {
        var product = _productRepository.GetProduct(productId);
        if (product == null)
            return NotFound($"El producto con el id {productId} no existe");

        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]//Se procesó con éxito y se creó un nuevo recurso
    [ProducesResponseType(StatusCodes.Status403Forbidden)]//Sin permisos
    [ProducesResponseType(StatusCodes.Status400BadRequest)]//Mal formada la solicitud
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]//Aplica, en autorizacion, si el usuario no esta autenticado o autorizado en estecaso
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]//Error del servidor
    public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        if (createProductDto == null)
        {
            return BadRequest(ModelState);
        }

        if (_productRepository.ProductExists(createProductDto.Name))
        {
            ModelState.AddModelError("CustomError", "EL producto ya existe");
            return BadRequest(ModelState);
        }

        if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
        {
            ModelState.AddModelError("CustomError", $"La categoria con el id: {createProductDto.CategoryId} no existe");
            return BadRequest(ModelState);
        }

        var product = _mapper.Map<Product>(createProductDto);

        if (!_productRepository.CreateProduct(product))
        {
            ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el producto {product.Name}");
            return StatusCode(500, ModelState);
        }

        var createdProduct = _productRepository.GetProduct(product.ProductId);
        var productDto = _mapper.Map<ProductDto>(createdProduct);

        return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productDto);
    }
}
