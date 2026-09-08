using System.Text.RegularExpressions;
using FluentAssertions;
using StangaNetLib.Core.Guards;

namespace StangaNetLib.Core.Tests.Guards;

public class GuardTests
{
    // --- Null ---

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

    // --- NullOrWhiteSpace ---

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

    // --- EmptyGuid ---

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

    // --- Negative / NegativeOrZero (generic INumber<T>) ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Against_NegativeOrZero_Int_WhenInvalid_ShouldThrow(int value)
    {
        var act = () => Guard.Against.NegativeOrZero(value, "value");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Against_NegativeOrZero_Int_WhenPositive_ShouldReturnValue()
    {
        Guard.Against.NegativeOrZero(5, "value").Should().Be(5);
    }

    [Fact]
    public void Against_Negative_Decimal_WhenNegative_ShouldThrow()
    {
        var act = () => Guard.Against.Negative(-0.01m, "amount");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Against_Negative_Decimal_WhenZeroOrPositive_ShouldReturnValue()
    {
        Guard.Against.Negative(0m, "amount").Should().Be(0m);
        Guard.Against.Negative(9.99m, "amount").Should().Be(9.99m);
    }

    [Fact]
    public void Against_NegativeOrZero_Long_WhenNegative_ShouldThrow()
    {
        var act = () => Guard.Against.NegativeOrZero(-1L, "value");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Against_NegativeOrZero_Long_WhenPositive_ShouldReturnValue()
    {
        Guard.Against.NegativeOrZero(100L, "value").Should().Be(100L);
    }

    // --- OutOfRange ---

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

    // --- NullOrEmpty (collection) ---

    [Fact]
    public void Against_NullOrEmpty_Collection_WhenEmpty_ShouldThrow()
    {
        var act = () => Guard.Against.NullOrEmpty(Array.Empty<int>(), "list");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_NullOrEmpty_Collection_WhenNull_ShouldThrow()
    {
        IEnumerable<int>? list = null;
        var act = () => Guard.Against.NullOrEmpty(list, "list");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_NullOrEmpty_Collection_WhenValid_ShouldReturnMaterializedList()
    {
        IEnumerable<int> lazy = Enumerable.Range(1, 3);
        var result = Guard.Against.NullOrEmpty(lazy, "list");

        result.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<int>>();
        result.Should().Equal(1, 2, 3);
    }

    // --- TooLong / TooShort ---

    [Fact]
    public void Against_TooLong_WhenExceedsMax_ShouldThrow()
    {
        var act = () => Guard.Against.TooLong("hello world", "s", 5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_TooLong_WhenNull_ShouldThrowArgumentNullException()
    {
        var act = () => Guard.Against.TooLong(null!, "s", 5);

        act.Should().Throw<ArgumentNullException>().WithParameterName("s");
    }

    [Fact]
    public void Against_TooShort_WhenBelowMin_ShouldThrow()
    {
        var act = () => Guard.Against.TooShort("hi", "s", 5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Against_TooShort_WhenNull_ShouldThrowArgumentNullException()
    {
        var act = () => Guard.Against.TooShort(null!, "s", 3);

        act.Should().Throw<ArgumentNullException>().WithParameterName("s");
    }

    [Fact]
    public void Against_TooLong_WhenWithinLimit_ShouldReturnValue()
    {
        Guard.Against.TooLong("hi", "s", 5).Should().Be("hi");
    }

    [Fact]
    public void Against_TooShort_WhenLongEnough_ShouldReturnValue()
    {
        Guard.Against.TooShort("hello", "s", 3).Should().Be("hello");
    }

    // --- Matches ---

    private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);

    [Fact]
    public void Against_Matches_WhenPatternMatches_ShouldReturnValue()
    {
        Guard.Against.Matches("12345", "code", DigitsOnly).Should().Be("12345");
    }

    [Fact]
    public void Against_Matches_WhenPatternDoesNotMatch_ShouldThrow()
    {
        var act = () => Guard.Against.Matches("abc", "code", DigitsOnly);

        act.Should().Throw<ArgumentException>().WithParameterName("code");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Against_Matches_WhenNullOrWhiteSpace_ShouldThrow(string? value)
    {
        var act = () => Guard.Against.Matches(value!, "code", DigitsOnly);

        act.Should().Throw<ArgumentException>();
    }

    // --- InvalidEnum ---

    private enum SampleStatus { Active = 1, Inactive = 2 }

    [Fact]
    public void Against_InvalidEnum_WhenDefined_ShouldReturnValue()
    {
        Guard.Against.InvalidEnum(SampleStatus.Active, "status").Should().Be(SampleStatus.Active);
    }

    [Fact]
    public void Against_InvalidEnum_WhenNotDefined_ShouldThrow()
    {
        var act = () => Guard.Against.InvalidEnum((SampleStatus)99, "status");

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("status");
    }

    // --- LengthBetween ---

    [Fact]
    public void Against_LengthBetween_WhenInRange_ShouldReturnValue()
    {
        Guard.Against.LengthBetween("hello", "s", 3, 10).Should().Be("hello");
    }

    [Theory]
    [InlineData("hi")]       // too short (min=3)
    [InlineData("hello world!")] // too long (max=10)
    public void Against_LengthBetween_WhenOutOfRange_ShouldThrow(string value)
    {
        var act = () => Guard.Against.LengthBetween(value, "s", 3, 10);

        act.Should().Throw<ArgumentException>().WithParameterName("s");
    }

    [Fact]
    public void Against_LengthBetween_WhenNull_ShouldThrowArgumentNullException()
    {
        var act = () => Guard.Against.LengthBetween(null!, "s", 1, 5);

        act.Should().Throw<ArgumentNullException>().WithParameterName("s");
    }
}
