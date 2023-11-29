using CoverGo.Task.Application;
using CoverGo.Task.Domain;

using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CoverGo.Task.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartQuery _cartQuery;
    private readonly ICartWriteRepository _cartWrite;
    private readonly IProductsQuery _productsQuery;
    private readonly IDiscountQuery _discountQuery;

    public CartController(ICartQuery cartQuery, ICartWriteRepository cartWrite, IProductsQuery productsQuery, IDiscountQuery discountQuery)
    {
        _cartQuery = cartQuery;
        _cartWrite = cartWrite;
        _productsQuery = productsQuery;
        _discountQuery = discountQuery;
    }

    [HttpGet(Name = "GetShoppingcarts")]
    [ProducesResponseType(typeof(List<ShoppingCart>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _cartQuery.ExecuteAsync());
    }

    [HttpPost("totals/{customerId}", Name = "GetCartTotals")]
    [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCartTotals([FromRoute] string customerId)
    {
        var discountRules = await _discountQuery.ExecuteAsync();
        var cart = await _cartQuery.GetByCustomerId(customerId);
        decimal totalPrice = 0;
        decimal totalDiscount = 0;
        if (cart != null) {
            foreach (var item in cart.Items)
            {
                totalPrice += item.product.Price * item.quantity;

                var discountRule = discountRules.FirstOrDefault(rule => rule.productName == item.product.Name);
                if (discountRule != null)
                {
                    var discountQuantity = item.quantity / discountRule.forEvery;
                    totalDiscount += item.product.Price * discountQuantity;
                }
            }
        }
        return Ok(new Dictionary<string, decimal> { { "totalPrice", totalPrice - totalDiscount }, { "totalDiscount", totalDiscount } });
    }

    [HttpPost("addToCart", Name = "AddTocart")]
    [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToCart(CartRequestObject cart)
    {
        try
        {
            string customerId = cart.customerId;
            string productName = cart.product;
            int quantity= cart.quantity;
            var product = await _productsQuery.GetByName(productName);
            if (product == null)
            {
                return NotFound($"Product with name {productName} not found.");
            }
            var cartItem = new CartItem { product = product, quantity = quantity };
            var shoppingCart = await _cartWrite.AddToCart(cartItem, customerId);

            return Ok(shoppingCart);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}
