using FluentAssertions;
using StangaNetLib.Core.Common;

namespace StangaNetLib.Core.Tests.Common;

public class ResultTests
{
    // --- Result<T> ---

    [Fact]
    public void Success_ShouldSetIsSuccessTrue_AndStoreValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_WithError_ShouldSetIsSuccessFalse_AndStoreError()
    {
        var error = Error.Validation("Email", "Invalid email");
        var result = Result<int>.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldStorePrimaryAndAll()
    {
        var errors = new[]
        {
            Error.Validation("Name", "Too short"),
            Error.Validation("Email", "Invalid")
        };
        var result = Result<string>.Failure(errors);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errors[0]);
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Match_OnSuccess_ShouldInvokeOnSuccess()
    {
        var result = Result<int>.Success(5);
        var outcome = result.Match(v => v * 2, _ => -1);
        outcome.Should().Be(10);
    }

    [Fact]
    public void Match_OnFailure_ShouldInvokeOnFailure()
    {
        var result = Result<int>.Failure(Error.Internal("err"));
        var outcome = result.Match(_ => 99, e => -1);
        outcome.Should().Be(-1);
    }

    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result<int>.Success(3).Map(v => v.ToString());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateErrors()
    {
        var error = Error.NotFound("User", 1);
        var result = Result<int>.Failure(error).Map(v => v.ToString());
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldReturnSuccess()
    {
        Result<string> result = "hello";
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldReturnFailure()
    {
        Result<string> result = Error.Unauthorized();
        result.IsFailure.Should().BeTrue();
    }

    // --- Result (non-generic) ---

    [Fact]
    public void Result_Success_ShouldHaveNoError()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Result_Failure_ShouldHaveError()
    {
        var result = Result.Failure(Error.Forbidden());
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Forbidden");
    }

    // --- Error record ---

    [Fact]
    public void Error_Validation_ShouldFormatCodeCorrectly()
    {
        var error = Error.Validation("Email", "bad format");
        error.Code.Should().Be("Validation.Email");
        error.Description.Should().Be("bad format");
    }

    [Fact]
    public void Error_NotFound_ShouldContainResourceAndId()
    {
        var error = Error.NotFound("User", 42);
        error.Code.Should().Be("User.NotFound");
        error.Description.Should().Contain("42");
    }

    [Fact]
    public void Error_None_ShouldHaveEmptyCodeAndDescription()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Description.Should().BeEmpty();
    }
}
