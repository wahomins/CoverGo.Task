using CoverGo.Task.Domain;

namespace CoverGo.Task.Application;

public interface IProductsQuery
{
    public ValueTask<List<Product>> ExecuteAsync();
    public ValueTask<Product?> GetByName(string name, CancellationToken cancellationToken = default);
}
