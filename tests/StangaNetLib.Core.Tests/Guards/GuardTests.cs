using FluentAssertions;
using StangaNetLib.Core.Guards;

namespace StangaNetLib.Core.Tests.Guards;

public class GuardTests
{
    [Fact]
    public void Against_Null_WhenNotNull_ShouldReturnValue()
    {
        var obj = new object();
        Guard.Against.Null(obj, "obj").Should().BeSameAs(obj);
    }

    [Fact]
    public void Against_Null_WhenNull_ShouldThrowArgumentNullException()
    {
        object? obj = null;
        var act = () => Guard.Against.Null(obj, "obj");
        act.Should().Throw<ArgumentNullException>().WithParameterName("obj");
    }

    [Fact]
    public void Against_NullOrWhiteSpace_WhenValid_ShouldReturnValue()
    {
        Guard.Against.NullOrWhiteSpace("hello", "s").Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Against_NullOrWhiteSpace_WhenInvalid_ShouldThrow(string? value)
    {
        var act = () => Guard.Against.NullOrWhiteSpace(value, "value");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_EmptyGuid_WhenEmpty_ShouldThrow()
    {
        var act = () => Guard.Against.EmptyGuid(Guid.Empty, "id");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_EmptyGuid_WhenValid_ShouldReturnValue()
    {
        var id = Guid.NewGuid();
        Guard.Against.EmptyGuid(id, "id").Should().Be(id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Against_NegativeOrZero_WhenInvalid_ShouldThrow(int value)
    {
        var act = () => Guard.Against.NegativeOrZero(value, "value");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Against_OutOfRange_WhenInRange_ShouldReturnValue()
    {
        Guard.Against.OutOfRange(5, "value", 1, 10).Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Against_OutOfRange_WhenOutOfRange_ShouldThrow(int value)
    {
        var act = () => Guard.Against.OutOfRange(value, "value", 1, 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Against_NullOrEmpty_Collection_WhenEmpty_ShouldThrow()
    {
        var act = () => Guard.Against.NullOrEmpty(Array.Empty<int>(), "list");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_TooLong_WhenExceedsMax_ShouldThrow()
    {
        var act = () => Guard.Against.TooLong("hello world", "s", 5);
        act.Should().Throw<ArgumentException>();
    }
}
