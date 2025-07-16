using APICatalog.DTOs;
using APICatalog.Interfaces;
using APICatalog.Models;
using APICatalog.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APICatalog.Controller;

[Route("[controller]")]
[ApiController]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly IMapper _mapper;

    public ProductsController(IUnitOfWork uof, IMapper mapper)
    {
        _uof = uof;
        _mapper = mapper;
    }

    private ActionResult<IEnumerable<ProductDTO>> GetProducts(PagedList<Product> products)
    {
        // Para incuir as informações no Header do Response, eu crio um objeto anônimo, ele contém as informações sobre paginação.
        var metadata = new
        {
            products.TotalCount,
            products.PageSize,
            products.CurrentPage,
            products.TotalPages,
            products.HasNext,
            products.HasPrevious
        };

        // Aqui eu adiciono um cabeçalho de respota personalizado.
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata)); // Estou serializando esses dados no formato Json e o incluindo no Header do Response.

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Ok(productsDto);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetPagination([FromQuery] ProductsParameters productsParameters)
    {
        var products = await _uof.ProductRepository.GetProductsAsync(productsParameters);
        return GetProducts(products);
    }

    [HttpGet("filter/price/pagination")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetFilteredProducts([FromQuery] ProductFilterPrice productFilterPrice)
    {
        var products = await _uof.ProductRepository.GetProductsFilteredByPriceAsync(productFilterPrice);
        return GetProducts(products);
    }

    [HttpGet("products/{id}")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProductsWithCategory(int id)
    {
        var products = await _uof.ProductRepository.GetProductsByCategoryAsync(id);

        if (products == null)
            return NotFound();

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Ok(productsDto);
    }

    /// <summary>
    /// Displays a list of products.
    /// </summary>
    /// <returns>Returns a list of product objects</returns>
    [HttpGet]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> Products()
    {
        var products = await _uof.ProductRepository.GetAllAsync();

        if (products == null)
            return NotFound("Products not found");

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Ok(productsDto);
    }

    /// <summary>
    /// Get a product by its id.
    /// </summary>
    /// <param name="id">Product code</param>
    /// <returns>Product objects</returns>
    [HttpGet("{id:int}", Name = "ObtainProduct")]
    public async Task<ActionResult<ProductDTO>> GetProduct(int? id)
    {
        if (id == null || id <= 0)
            return BadRequest("Invalid Id");

        var product = await _uof.ProductRepository.GetAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound("Product not found");

        var productDto = _mapper.Map<ProductDTO>(product);

        return Ok(productDto);
    }

    [HttpPost("products")]
    public async Task<ActionResult<ProductDTO>> CreateProduct(ProductDTO productDto)
    {

        if (productDto == null)
            return BadRequest("Product cannot be null");

        var product = _mapper.Map<Product>(productDto);

        var newProduct = _uof.ProductRepository.Create(product);
        await _uof.CommitAsync();

        var newProductDto = _mapper.Map<ProductDTO>(newProduct);

        return new CreatedAtRouteResult("ObtainProduct",
            new { id = newProductDto.ProductId }, newProductDto);
    }

    [HttpPatch("{id}/UpdatePartial")]
    public async Task<ActionResult<ProductDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProductDTOUpdateRequest> patchProductDto) // JsonPatchDocument é um parâmetro que representa um documento de patch Json, utilizado para atualizar parcialmente um recurso.
    {
        if (patchProductDto == null || id <= 0)
            return BadRequest("Product cannot be null");

        var product = await _uof.ProductRepository.GetAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound("$Product with id {id} not found.");

        var productUpdateRequest = _mapper.Map<ProductDTOUpdateRequest>(product);

        // Aplicar as alterações parciais:
        patchProductDto.ApplyTo(productUpdateRequest, ModelState);

        // Verificar o modelState para verificar se contém erros de validação após as alterações aplicadas com o ApplyTo().
        if (!ModelState.IsValid || !TryValidateModel(productUpdateRequest)) // O TryValidateModel vai validar o modelo com base nas regras que definimos no ProductUpdateRequest.
            return BadRequest(ModelState);

        _mapper.Map(productUpdateRequest, product);

        _uof.ProductRepository.Update(product);
        await _uof.CommitAsync();

        return Ok(_mapper.Map<ProductDTOUpdateResponse>(product));
    }


    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDTO>> UpdateProduct(int id, ProductDTO productDto)
    {

        if (id <= 0)
            return NotFound();

        if (id != productDto.ProductId)
            return BadRequest("Product ID mismatch");

        var product = _mapper.Map<Product>(productDto);

        var updatedProduct = _uof.ProductRepository.Update(product);
        await _uof.CommitAsync();

        var updatedProductDto = _mapper.Map<ProductDTO>(updatedProduct);

        return Ok(updatedProductDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProductDTO>> DeleteProduct(int id)
    {
        if (id <= 0)
            return NotFound();

        var product = await _uof.ProductRepository.GetAsync(p => p.ProductId == id);

        if (product == null)
            return NotFound($"Product with id {id} not found.");

        var deletedProduct = _uof.ProductRepository.Delete(product);
        await _uof.CommitAsync();

        var deletedProductDto = _mapper.Map<ProductDTO>(deletedProduct);

        return Ok("Product deleted successfully");
    }

}
