using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CoverGo.Task.Domain;

public class CartItem
{
    public required Product product { get; set; }
    public required int quantity { get; set; }
}

public class ShoppingCart
{
    public required string CustomerId { get; set; }
    public List<CartItem> Items { get; set;} = new List<CartItem>();
}
