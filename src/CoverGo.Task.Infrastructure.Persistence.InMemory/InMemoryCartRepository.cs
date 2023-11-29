using System.Collections.Immutable;

using CoverGo.Task.Application;
using CoverGo.Task.Domain;

namespace CoverGo.Task.Infrastructure.Persistence.InMemory;

internal class InMemoryCartRepository : ICartQuery, ICartWriteRepository
{
    private static ImmutableList<ShoppingCart> _seedwork = new List<ShoppingCart> { }.ToImmutableList();

    public ValueTask<ShoppingCart> GetByCustomerId(string id, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_seedwork.Single(it => it.CustomerId == id));
    }
    public ValueTask<List<ShoppingCart>> ExecuteAsync()
    {
        return ValueTask.FromResult(_seedwork.ToList());
    }

    public ValueTask<ShoppingCart> AddToCart(CartItem item, string customerId, CancellationToken cancellationToken = default)
    {
        var cart = _seedwork.FirstOrDefault(it => it.CustomerId == customerId);
        if (cart == null)
        {
            cart = new ShoppingCart { CustomerId = customerId };
            _seedwork = _seedwork.Add(cart);
        }
        var cartItem = cart.Items.FirstOrDefault(i => i.product == item.product);
        if (cartItem != null)
        {
            cartItem.quantity += item.quantity;
        }
        else
        {
            cart.Items.Add(item);
        }
        return ValueTask.FromResult(cart);
    }
}
