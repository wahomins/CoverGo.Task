using CoverGo.Task.Domain;

namespace CoverGo.Task.Application;

public interface IDiscountWriteRepository
{
    public ValueTask<DiscountRule> AddDiscountRule(DiscountRule rule, CancellationToken cancellationToken = default);
}
