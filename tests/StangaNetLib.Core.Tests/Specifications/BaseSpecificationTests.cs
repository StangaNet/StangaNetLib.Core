using System.Linq.Expressions;
using FluentAssertions;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Specifications;

namespace StangaNetLib.Core.Tests.Specifications;

file class SampleItem : Entity
{
    public int Value { get; }
    public SampleItem(int value) { Value = value; }
}

file class ValueGreaterThanSpec : BaseSpecification<SampleItem>
{
    public ValueGreaterThanSpec(int threshold)
        : base(item => item.Value > threshold) { }

    public void AddTestInclude(Expression<Func<SampleItem, object>> expr) => AddInclude(expr);
    public void SetAscending() => ApplyOrderBy(x => x.Value);
    public void SetDescending() => ApplyOrderByDescending(x => x.Value);
    public void SetPaging(int skip, int take) => ApplyPaging(skip, take);
}

public class BaseSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WhenCriteriaMet_ShouldReturnTrue()
    {
        var spec = new ValueGreaterThanSpec(5);
        var item = new SampleItem(10);

        spec.IsSatisfiedBy(item).Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WhenCriteriaNotMet_ShouldReturnFalse()
    {
        var spec = new ValueGreaterThanSpec(5);
        var item = new SampleItem(3);

        spec.IsSatisfiedBy(item).Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_CalledMultipleTimes_ShouldReuseCompiledDelegate()
    {
        var spec = new ValueGreaterThanSpec(0);
        var item = new SampleItem(1);

        // Calling multiple times exercises the cache path
        for (var i = 0; i < 5; i++)
            spec.IsSatisfiedBy(item).Should().BeTrue();
    }

    [Fact]
    public void IsPagingEnabled_ShouldBeFalse_ByDefault()
    {
        var spec = new ValueGreaterThanSpec(0);

        spec.IsPagingEnabled.Should().BeFalse();
    }

    [Fact]
    public void ApplyPaging_ShouldSetSkipTakeAndEnablePaging()
    {
        var spec = new ValueGreaterThanSpec(0);
        spec.SetPaging(skip: 10, take: 5);

        spec.IsPagingEnabled.Should().BeTrue();
        spec.Skip.Should().Be(10);
        spec.Take.Should().Be(5);
    }

    [Fact]
    public void ApplyOrderBy_ShouldSetOrderBy_AndClearOrderByDescending()
    {
        var spec = new ValueGreaterThanSpec(0);
        spec.SetDescending();
        spec.SetAscending();

        spec.OrderBy.Should().NotBeNull();
        spec.OrderByDescending.Should().BeNull();
    }

    [Fact]
    public void ApplyOrderByDescending_ShouldSetOrderByDescending_AndClearOrderBy()
    {
        var spec = new ValueGreaterThanSpec(0);
        spec.SetAscending();
        spec.SetDescending();

        spec.OrderByDescending.Should().NotBeNull();
        spec.OrderBy.Should().BeNull();
    }

    [Fact]
    public void Includes_ShouldBeIReadOnlyList_NotMutableList()
    {
        var spec = new ValueGreaterThanSpec(0);

        spec.Includes.Should().BeAssignableTo<IReadOnlyList<Expression<Func<SampleItem, object>>>>();
    }

    [Fact]
    public void AddInclude_ShouldAppearInIncludes()
    {
        var spec = new ValueGreaterThanSpec(0);
        spec.AddTestInclude(x => x.Value);

        spec.Includes.Should().HaveCount(1);
    }

    // --- Composition operators ---

    [Fact]
    public void And_BothSatisfied_ShouldReturnTrue()
    {
        var greaterThan3 = new ValueGreaterThanSpec(3);
        var greaterThan1 = new ValueGreaterThanSpec(1);
        var combined = greaterThan1.And(greaterThan3);

        combined.IsSatisfiedBy(new SampleItem(5)).Should().BeTrue();
    }

    [Fact]
    public void And_OnlyOneSatisfied_ShouldReturnFalse()
    {
        var greaterThan3 = new ValueGreaterThanSpec(3);
        var greaterThan10 = new ValueGreaterThanSpec(10);
        var combined = greaterThan3.And(greaterThan10);

        combined.IsSatisfiedBy(new SampleItem(5)).Should().BeFalse();
    }

    [Fact]
    public void Or_AtLeastOneSatisfied_ShouldReturnTrue()
    {
        var greaterThan3 = new ValueGreaterThanSpec(3);
        var greaterThan10 = new ValueGreaterThanSpec(10);
        var combined = greaterThan3.Or(greaterThan10);

        combined.IsSatisfiedBy(new SampleItem(5)).Should().BeTrue();
    }

    [Fact]
    public void Or_NeitherSatisfied_ShouldReturnFalse()
    {
        var greaterThan5 = new ValueGreaterThanSpec(5);
        var greaterThan10 = new ValueGreaterThanSpec(10);
        var combined = greaterThan5.Or(greaterThan10);

        combined.IsSatisfiedBy(new SampleItem(3)).Should().BeFalse();
    }

    [Fact]
    public void Not_WhenOriginalSatisfied_ShouldReturnFalse()
    {
        var greaterThan3 = new ValueGreaterThanSpec(3);
        var negated = greaterThan3.Not();

        negated.IsSatisfiedBy(new SampleItem(5)).Should().BeFalse();
    }

    [Fact]
    public void Not_WhenOriginalNotSatisfied_ShouldReturnTrue()
    {
        var greaterThan3 = new ValueGreaterThanSpec(3);
        var negated = greaterThan3.Not();

        negated.IsSatisfiedBy(new SampleItem(1)).Should().BeTrue();
    }

    [Fact]
    public void And_Criteria_ShouldBeEFCoreTranslatable()
    {
        var left = new ValueGreaterThanSpec(1);
        var right = new ValueGreaterThanSpec(3);
        var combined = left.And(right);

        var items = new[] { new SampleItem(2), new SampleItem(4), new SampleItem(5) }.AsQueryable();
        var filtered = items.Where(combined.Criteria).ToList();

        filtered.Should().HaveCount(2).And.OnlyContain(x => x.Value > 3);
    }

    [Fact]
    public void Or_Criteria_ShouldBeEFCoreTranslatable()
    {
        var left = new ValueGreaterThanSpec(10);
        var right = new ValueGreaterThanSpec(3);
        var combined = left.Or(right);

        var items = new[] { new SampleItem(2), new SampleItem(5), new SampleItem(15) }.AsQueryable();
        var filtered = items.Where(combined.Criteria).ToList();

        filtered.Should().HaveCount(2).And.OnlyContain(x => x.Value > 3 || x.Value > 10);
    }
}
