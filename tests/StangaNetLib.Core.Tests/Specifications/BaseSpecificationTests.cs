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
    public void IsPagingEnabled_ShouldBeFalse_ByDefault()
    {
        var spec = new ValueGreaterThanSpec(0);
        spec.IsPagingEnabled.Should().BeFalse();
    }
}
