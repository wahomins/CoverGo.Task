using CoverGo.Task.Domain;

namespace CoverGo.Task.Application;

public interface ICartWriteRepository
{
    public ValueTask<ShoppingCart> AddToCart(CartItem item, string customerId, CancellationToken cancellationToken = default);
}
