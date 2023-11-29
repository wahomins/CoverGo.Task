using CoverGo.Task.Application;
using CoverGo.Task.Infrastructure.Persistence.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

public static class PersistenceInMemoryDI
{
    public static IServiceCollection AddInMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IProductsQuery, InMemoryProductsRepository>();
        services.AddSingleton<ICartQuery, InMemoryCartRepository>();
        services.AddSingleton<IProductsWriteRepository, InMemoryProductsRepository>();
        services.AddSingleton<ICartWriteRepository, InMemoryCartRepository>();
        services.AddSingleton<IDiscountWriteRepository, InMemoryDiscountRepository>();
        services.AddSingleton<IDiscountQuery, InMemoryDiscountRepository>();
        return services;
    }
}
