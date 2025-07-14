using APICatalog.DTOs;
using APICatalog.DTOs.Mappings;
using APICatalog.Filters;
using APICatalog.Interfaces;
using APICatalog.Models;
using APICatalog.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace APICatalog.Controller;

[Route("[controller]")]
[ApiController]
[EnableRateLimiting("fixedWindow")]
[Produces("application/json")] // Definir o tipo de retorno
[ApiConventionType(typeof(DefaultApiConventions))] // Aplica um conjunto padrão de convenções com todos os possíveis Status de retorno dos meus endpoints.
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly ILogger<CategoriesController> _logger;
    private readonly IConfiguration _configuration;

    public CategoriesController(IUnitOfWork uof, ILogger<CategoriesController> logger, IConfiguration configuration)
    {
        _uof = uof;
        _logger = logger;
        _configuration = configuration;
    }
    private ActionResult<IEnumerable<CategoryDTO>> GetCategories(PagedList<Category> categories)
    {
        var metadata = new
        {
            categories.TotalCount,
            categories.PageSize,
            categories.CurrentPage,
            categories.TotalPages,
            categories.HasNext,
            categories.HasPrevious
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriesDto = categories.ToCategoryDTOList();

        return Ok(categoriesDto);
    }

    // Exemplo: Lendo arquivo de configuração
    [Authorize]
    [HttpGet("readConfig")]
    public string GetValue()
    {
        var value1 = _configuration["chave1"];
        var value2 = _configuration["chave2"];

        var sec1 = _configuration["secao1:chave2"];

        return $"Exemplo lendo arquivo de configuração: \nChave1 = {value1} \nChave2 = {value2} \nSeção1 => Chave2 = {sec1}";
    }

    [HttpGet("pagination")]
    [DisableRateLimiting]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetPagination([FromQuery] CategoriesParameters categoriesParameters)
    {
        var categories = await _uof.CategoryRepository.GetCategoriesAsync(categoriesParameters);
        return GetCategories(categories);
    }

    [HttpGet("filter/name/categories")]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetFilteredCategories([FromQuery] CategoriesFilterName categoriesFilterName)
    {
        var categories = await _uof.CategoryRepository.GetCategoriesFilteredByNameAsync(categoriesFilterName);
        return GetCategories(categories);
    }

    /// <summary>
    /// Get a list of category objects.
    /// </summary>
    /// <returns>A list of category objects</returns>
    [HttpGet]
    [ServiceFilter(typeof(ApiLoggingFilter))] // Aplicando o filtro customizado
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> Categories()
    {
        var categories = await _uof.CategoryRepository.GetAllAsync();

        if (categories == null)
            return NotFound("Categories not found");

        var categoriesDto = categories.ToCategoryDTOList();

        return Ok(categoriesDto);
    }

    /// <summary>
    /// Get a category by its id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Category objects</returns>
    [HttpGet("{id:int}", Name = "ObtainCategory")]
    public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
    {

        if (id <= 0)
            return NotFound();

        var category = await _uof.CategoryRepository.GetAsync(c => c.CategoryId == id);

        if (category == null)
        {
            _logger.LogWarning($"Category with id {id} not found.");
            return NotFound($"Category with id {id} not found.");
        }

        var categoryDto = category.ToCategoryDTO();

        return Ok(categoryDto);
    }

    /// <summary>
    /// Includes a new category.
    /// </summary>
    /// <remarks>
    /// Request Example:
    ///     POST api/categories
    ///     {
    ///         "categoryId": 1,
    ///         "name:" "category1",
    ///         "imageUrl": "http://teste.net/1.jpg"
    ///     }
    /// </remarks>
    /// <param name="categoryDto">Category objects</param>
    /// <returns>The included category object</returns>
    /// <remarks>Returns an included category object</remarks>
    [HttpPost("categories")]
    [ProducesResponseType(StatusCodes.Status201Created)] // Explicitar os tipos de retorno possíveis
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryDTO>> CreateCategory(CategoryDTO categoryDto)
    {

        if (categoryDto == null)
            return BadRequest("Category cannot be null");

        var category = categoryDto.ToCategory();

        var newCategory = _uof.CategoryRepository.Create(category);
        await _uof.CommitAsync();

        var newCategoryDto = newCategory.ToCategoryDTO();

        return new CreatedAtRouteResult("ObtainCategory",
            new { id = newCategoryDto.CategoryId }, newCategoryDto);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<ActionResult<CategoryDTO>> UpdateCategory(int id, CategoryDTO categoryDto)
    {
        if (id <= 0)
            return NotFound();

        if (id != categoryDto.CategoryId)
            return BadRequest("Category ID mismatch");

        var category = categoryDto.ToCategory();

        var categoryUpdate = _uof.CategoryRepository.Update(category);
        await _uof.CommitAsync();

        var newCategoryDto = categoryUpdate.ToCategoryDTO();
        return Ok(category);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoryDTO>> DeleteCategory(int id)
    {
        if (id <= 0)
            return NotFound();

        var category = await _uof.CategoryRepository.GetAsync(c => c.CategoryId == id);

        if (category == null)
        {
            _logger.LogWarning($"Category with id {id} not found.");
            return NotFound($"Category with id {id} not found.");
        }

        var categoryDeleted = _uof.CategoryRepository.Delete(category);
        await _uof.CommitAsync();

        var catetegoryDeletedDto = categoryDeleted.ToCategoryDTO();

        return Ok("Category deleted successfully");
    }
}
