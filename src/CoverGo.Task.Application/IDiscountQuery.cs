using CoverGo.Task.Domain;

namespace CoverGo.Task.Application;

public interface IDiscountQuery
{
    public ValueTask<List<DiscountRule>> ExecuteAsync();
}
