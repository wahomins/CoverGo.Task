using System.Collections.Immutable;

using CoverGo.Task.Application;
using CoverGo.Task.Domain;

namespace CoverGo.Task.Infrastructure.Persistence.InMemory;

internal class InMemoryProductsRepository : IProductsQuery, IProductsWriteRepository
{
    private static ImmutableList<Product> _seedwork = new List<Product> { }.ToImmutableList();

    public ValueTask<Product> AddProduct(Product product, CancellationToken cancellationToken = default)
    {
        if (_seedwork.Any(p => p.Name == product.Name))
        {
            throw new Exception("Product with the same name already exists.");
        }
        _seedwork = _seedwork.Add(product);
        return new ValueTask<Product>(product);
    }

    public ValueTask<Product?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_seedwork.SingleOrDefault(it => it.Name == name));
    }

    public ValueTask<List<Product>> ExecuteAsync()
    {
        return ValueTask.FromResult(_seedwork.ToList());
    }
}
