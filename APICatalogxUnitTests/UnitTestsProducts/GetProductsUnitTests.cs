using APICatalog.Controller;
using APICatalog.DTOs;
using APICatalogxUnitTests.UnitTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogxUnitTests.UnitTestsProducts;

public class GetProductsUnitTests : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _productsController;

    public GetProductsUnitTests(ProductsUnitTestController controller)
    {
        _productsController = new ProductsController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task GetProductById_Return_OkResult()
    {
        //Arange
        var productId = 2; ;

        //Act
        var data = await _productsController.GetProduct(productId);

        //Assert com xUnit
        var okResult = Assert.IsType<OkObjectResult>(data.Result); // Verifica se o resultado é do tipo OkObjectResult.
        Assert.Equal(200, okResult.StatusCode); // Verifica se o código de status do okObjectResult é 200.
    }

    [Fact]
    public async Task GetProductById_Return_NotFound()
    {
        //Arange
        var productId = 999;

        //Act
        var data = await _productsController.GetProduct(productId);

        //Assert com FluentAssertions
        data.Result.Should().BeOfType<NotFoundObjectResult>()
                    .Which.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetProductById_Return_BadRequest()
    {
        var data = await _productsController.GetProduct(null);

        data.Result.Should().BeOfType<BadRequestObjectResult>()
                    .Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetProducts_Return_ListOfProductDto()
    {
        var data = await _productsController.Products();

        data.Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeAssignableTo<IEnumerable<ProductDTO>>()
                    .And.NotBeNull();

    }
}
