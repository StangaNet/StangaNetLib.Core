using FluentAssertions;
using StangaNetLib.Core.Validators;

namespace StangaNetLib.Core.Tests.Validators;

public class ValidationResultTests
{
    [Fact]
    public void Success_ShouldHaveNoErrors()
    {
        var result = ValidationResult.Success();
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_SingleError_ShouldContainIt()
    {
        var result = ValidationResult.Failure("Name is required");
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle("Name is required");
    }

    [Fact]
    public void Failure_MultipleErrors_ShouldContainAll()
    {
        var result = ValidationResult.Failure(["Err1", "Err2"]);
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void GetErrorMessage_ShouldJoinWithSemicolon()
    {
        var result = ValidationResult.Failure(["A", "B"]);
        result.GetErrorMessage().Should().Be("A; B");
    }

    [Fact]
    public void ToResult_OnSuccess_ShouldReturnSuccessResult()
    {
        var result = ValidationResult.Success().ToResult();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_OnFailure_ShouldReturnFailureResult()
    {
        var result = ValidationResult.Failure("bad").ToResult();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
    }
}
