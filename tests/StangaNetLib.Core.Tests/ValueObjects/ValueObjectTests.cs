using FluentAssertions;
using StangaNetLib.Core.ValueObjects;

namespace StangaNetLib.Core.Tests.ValueObjects;

file class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

public class ValueObjectTests
{
    [Fact]
    public void TwoValueObjects_WithSameComponents_ShouldBeEqual()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(10m, "EUR");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void TwoValueObjects_WithDifferentComponents_ShouldNotBeEqual()
    {
        var a = new Money(10m, "EUR");
        var b = new Money(10m, "USD");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ShouldBeEqual_ForEqualObjects()
    {
        var a = new Money(5m, "USD");
        var b = new Money(5m, "USD");
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Null_ShouldNotEqual_AnyValueObject()
    {
        var a = new Money(1m, "EUR");
        (a == null).Should().BeFalse();
        (null == a).Should().BeFalse();
    }
}
