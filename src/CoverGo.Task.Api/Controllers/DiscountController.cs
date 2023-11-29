using CoverGo.Task.Application;
using CoverGo.Task.Domain;

using Microsoft.AspNetCore.Mvc;

namespace CoverGo.Task.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountQuery _discountQuery;
    private readonly IDiscountWriteRepository _discountWrite;
    private readonly IProductsQuery _productsQuery;

    public DiscountController(IDiscountQuery discountQuery, IDiscountWriteRepository discountWrite, IProductsQuery productsQuery)
    {
        _discountQuery = discountQuery;
        _discountWrite = discountWrite;
        _productsQuery = productsQuery;
    }

    [HttpGet(Name = "GetRules")]
    [ProducesResponseType(typeof(List<DiscountRule>), StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<List<DiscountRule>>> GetAll()
    {
        return await _discountQuery.ExecuteAsync();
    }

    [HttpPost(Name = "AddRule")]
    [ProducesResponseType(typeof(DiscountRule), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRule(DiscountRule rule)
    {
        try
        {
            var product = await _productsQuery.GetByName(rule.productName);
            if (product == null)
            {
                return NotFound($"Product with name {rule.productName} not found.");
            }
            if (rule.forEvery < 1)
            {
                return BadRequest("forEvery in rule should be a positive number");
            }
            return Ok(await _discountWrite.AddDiscountRule(rule));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
