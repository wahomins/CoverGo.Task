using System.Collections.Immutable;

using CoverGo.Task.Application;
using CoverGo.Task.Domain;

namespace CoverGo.Task.Infrastructure.Persistence.InMemory;

internal class InMemoryDiscountRepository : IDiscountWriteRepository, IDiscountQuery
{
    private static ImmutableList<DiscountRule> _seedwork = new List<DiscountRule> { }.ToImmutableList();

    public ValueTask<DiscountRule> AddDiscountRule(DiscountRule rule, CancellationToken cancellationToken = default)
    {
        if (_seedwork.Any(p => p.productName == rule.productName))
        {
            throw new Exception("Rule already exists.");
        }
        _seedwork = _seedwork.Add(rule);
        return new ValueTask<DiscountRule>(rule);
    }
    public ValueTask<List<DiscountRule>> ExecuteAsync()
    {
        return ValueTask.FromResult(_seedwork.ToList());
    }
}
