using APICatalog.Controller;
using APICatalog.DTOs;
using APICatalogxUnitTests.UnitTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogxUnitTests.UnitTestsProducts;

public class PostProductsUnitTests : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _productsController;

    public PostProductsUnitTests(ProductsUnitTestController controller)
    {
        _productsController = new ProductsController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task PostProduct_Return_CreatedStatusCode()
    {
        var newProductDto = new ProductDTO
        {
            Name = "Test",
            Description = "Description test",
            Price = 10.99m,
            ImageUrl = "imageTest.jpg",
            CategoryId = 2
        };

        var data = await _productsController.CreateProduct(newProductDto);

        var createdResult = data.Result.Should().BeOfType<CreatedAtRouteResult>();
        createdResult.Subject.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task PostProduct_Return_BadRequest()
    {
        var data = await _productsController.CreateProduct(null);

        data.Result.Should().BeOfType<BadRequestObjectResult>()
                    .Which.StatusCode.Should().Be(400);
    }
}
