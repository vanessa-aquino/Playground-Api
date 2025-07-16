using APICatalog.Controller;
using APICatalog.DTOs;
using APICatalogxUnitTests.UnitTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogxUnitTests.UnitTestsProducts;
public class PutProductsUnitTests : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _productsController;

    public PutProductsUnitTests(ProductsUnitTestController controller)
    {
        _productsController = new ProductsController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task PutProduct_Return_OkResult()
    {
        var productId = 1;

        var UpdateProductDto = new ProductDTO
        {
            ProductId = productId,
            Name = "Test Updated",
            Description = "My new description",
            ImageUrl = "newImage.jpg",
            CategoryId = 2
        };

        var result = await _productsController.UpdateProduct(productId, UpdateProductDto) as ActionResult<ProductDTO>;

        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PutProduct_Return_BadRequest()
    {
        var productId = 200;

        var UpdateProductDto = new ProductDTO
        {
            ProductId = 2,
            Name = "Test Updated",
            Description = "My new description",
            ImageUrl = "newImage.jpg",
            CategoryId = 2
        };

        var actionResult = await _productsController.UpdateProduct(productId, UpdateProductDto);
        var result = actionResult.Result;    

        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.StatusCode.Should().Be(400);
    }
}
