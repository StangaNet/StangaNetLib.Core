using FluentAssertions;
using StangaNetLib.Core.Common;
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

    // --- Failure from string (convenience overload) ---

    [Fact]
    public void Failure_FromString_ShouldCreateStructuredError()
    {
        var result = ValidationResult.Failure("Name is required");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Description == "Name is required");
        result.Errors.Single().Code.Should().Be("Validation");
    }

    [Fact]
    public void Failure_FromStrings_ShouldCreateMultipleStructuredErrors()
    {
        var result = ValidationResult.Failure(["Err1", "Err2"]);

        result.Errors.Should().HaveCount(2);
        result.Errors.Should().AllSatisfy(e => e.Code.Should().Be("Validation"));
    }

    // --- Failure from Error ---

    [Fact]
    public void Failure_FromError_ShouldStoreItDirectly()
    {
        var error = Error.Validation("Email", "Invalid format");
        var result = ValidationResult.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Fact]
    public void Failure_FromErrors_ShouldStoreAll()
    {
        var errors = new[]
        {
            Error.Validation("Name", "Required"),
            Error.Validation("Email", "Invalid")
        };
        var result = ValidationResult.Failure(errors);

        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(errors[0]);
        result.Errors.Should().Contain(errors[1]);
    }

    // --- GetErrorMessage ---

    [Fact]
    public void GetErrorMessage_ShouldJoinDescriptionsWithSemicolon()
    {
        var result = ValidationResult.Failure(["A", "B"]);

        result.GetErrorMessage().Should().Be("A; B");
    }

    // --- ToResult ---

    [Fact]
    public void ToResult_OnSuccess_ShouldReturnSuccessResult()
    {
        var result = ValidationResult.Success().ToResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_OnFailure_ShouldReturnFailureResultWithStructuredErrors()
    {
        var result = ValidationResult.Failure("bad").ToResult();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation");
        result.Error.Description.Should().Be("bad");
    }

    [Fact]
    public void ToResult_OnFailure_WithMultipleErrors_ShouldPropagateAll()
    {
        var result = ValidationResult.Failure(["e1", "e2"]).ToResult();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void ToResultTyped_OnFailure_ShouldReturnFailureResult()
    {
        var result = ValidationResult.Failure("bad").ToResult<string>();

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("bad");
    }

    [Fact]
    public void ToResultTyped_OnSuccess_ShouldThrowInvalidOperationException()
    {
        var act = () => ValidationResult.Success().ToResult<string>();

        act.Should().Throw<InvalidOperationException>();
    }

    // --- Combine ---

    [Fact]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        var combined = ValidationResult.Combine(
            ValidationResult.Success(),
            ValidationResult.Success());

        combined.IsSuccess.Should().BeTrue();
        combined.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Combine_WithFailures_ShouldAggregateAllErrors()
    {
        var combined = ValidationResult.Combine(
            ValidationResult.Failure("err1"),
            ValidationResult.Success(),
            ValidationResult.Failure("err2"));

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_AllFailures_ShouldCollectEveryError()
    {
        var combined = ValidationResult.Combine(
            ValidationResult.Failure(Error.Validation("Name", "Required")),
            ValidationResult.Failure(Error.Validation("Email", "Invalid")));

        combined.Errors.Should().HaveCount(2);
        combined.GetErrorMessage().Should().Contain("Required");
        combined.GetErrorMessage().Should().Contain("Invalid");
    }
}
