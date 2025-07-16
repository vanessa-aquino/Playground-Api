using APICatalog.Controller;
using APICatalogxUnitTests.UnitTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogxUnitTests.UnitTestsProducts;

public class DeleteProductsUnitTests : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _productsController;

    public DeleteProductsUnitTests(ProductsUnitTestController controller)
    {
        _productsController = new ProductsController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task DeleteProductById_Return_OkResult()
    {
        var productId = 2;

        var result = await _productsController.DeleteProduct(productId);

        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteProductById_Return_NotFound()
    {
        var productId = 99;

        var result = await _productsController.DeleteProduct(productId);

        result.Should().NotBeNull();
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

}
