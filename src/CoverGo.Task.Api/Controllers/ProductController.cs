using CoverGo.Task.Application;
using CoverGo.Task.Domain;

using Microsoft.AspNetCore.Mvc;

namespace CoverGo.Task.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductsQuery _productsQuery;
    private readonly IProductsWriteRepository _productsWrite;

    public ProductsController(IProductsQuery productsQuery, IProductsWriteRepository productsWrite)
    {
        _productsQuery = productsQuery;
        _productsWrite = productsWrite;
    }

    [HttpGet(Name = "GetProducts")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _productsQuery.ExecuteAsync());
    }

    [HttpPost(Name = "AddProducts")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddProduct(Product product)
    {
        if (string.IsNullOrEmpty(product.Name))
        {
            return BadRequest("Product name cannot be empty.");
        }
        if (product.Price < 0)
        {
            return BadRequest("Price cannot be negative.");
        }
        try
        {
            return Ok(await _productsWrite.AddProduct(product));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
