using CoverGo.Task.Domain;

namespace CoverGo.Task.Application;

public interface IProductsWriteRepository
{
    public ValueTask<Product> AddProduct(Product product, CancellationToken cancellationToken = default);
}
