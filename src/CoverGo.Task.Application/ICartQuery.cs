using CoverGo.Task.Domain;

namespace CoverGo.Task.Application;

public interface ICartQuery
{
    public ValueTask<List<ShoppingCart>> ExecuteAsync();
    public ValueTask<ShoppingCart> GetByCustomerId(string id, CancellationToken cancellationToken = default);
}
