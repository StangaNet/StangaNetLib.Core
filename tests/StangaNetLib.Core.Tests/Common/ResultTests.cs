using FluentAssertions;
using StangaNetLib.Core.Common;

namespace StangaNetLib.Core.Tests.Common;

public class ResultTests
{
    // --- Result<T> factory ---

    [Fact]
    public void Success_ShouldSetIsSuccessTrue_AndStoreValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
        result.Errors.Should().BeEmpty();
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

    // --- Combinators ---

    [Fact]
    public void Match_OnSuccess_ShouldInvokeOnSuccess()
    {
        var result = Result<int>.Success(5);
        var outcome = result.Match(v => v * 2, (Error _) => -1);

        outcome.Should().Be(10);
    }

    [Fact]
    public void Match_OnFailure_ShouldInvokeOnFailure()
    {
        var result = Result<int>.Failure(Error.Internal("err"));
        var outcome = result.Match(_ => 99, (Error _) => -1);

        outcome.Should().Be(-1);
    }

    [Fact]
    public void MatchAll_OnFailure_ShouldReceiveAllErrors()
    {
        var errors = new[] { Error.Validation("A", "e1"), Error.Validation("B", "e2") };
        var result = Result<int>.Failure(errors);

        var count = result.MatchAll(_ => 0, errs => errs.Count);

        count.Should().Be(2);
    }

    [Fact]
    public void MatchAll_OnSuccess_ShouldInvokeOnSuccess()
    {
        var result = Result<int>.Success(7);
        var outcome = result.MatchAll(v => v, errs => -errs.Count);

        outcome.Should().Be(7);
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
    public async Task MapAsync_OnSuccess_ShouldTransformValue()
    {
        var result = await Result<int>.Success(4).MapAsync(v => Task.FromResult(v * 10));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(40);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ShouldChainOperation()
    {
        var result = await Result<int>.Success(3)
            .BindAsync(v => Task.FromResult(Result<string>.Success(v.ToString())));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task BindAsync_OnFailure_ShouldPropagateErrors()
    {
        var error = Error.Internal("fail");
        var result = await Result<int>.Failure(error)
            .BindAsync(v => Task.FromResult(Result<string>.Success(v.ToString())));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    // --- Implicit conversions ---

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

    [Fact]
    public void ImplicitConversion_FromNull_ShouldThrowArgumentNullException()
    {
        string? value = null;
        var act = () => { Result<string> _ = value!; };

        act.Should().Throw<ArgumentNullException>();
    }

    // --- Result.Combine ---

    [Fact]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        var combined = Result.Combine(Result.Success(), Result.Success());

        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_WithFailures_ShouldAggregateAllErrors()
    {
        var combined = Result.Combine(
            Result.Failure(Error.Validation("A", "err1")),
            Result.Failure(Error.Validation("B", "err2")),
            Result.Success());

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void CombineTyped_AllSuccess_ShouldReturnAllValues()
    {
        var combined = Result.Combine(
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3));

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void CombineTyped_WithFailure_ShouldAggregateErrors()
    {
        var combined = Result.Combine(
            Result<int>.Success(1),
            Result<int>.Failure(Error.Validation("X", "bad")));

        combined.IsFailure.Should().BeTrue();
        combined.Errors.Should().HaveCount(1);
    }

    // --- Result (non-generic) ---

    [Fact]
    public void Result_Success_ShouldHaveNoError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(Error.None);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Result_Failure_ShouldHaveError()
    {
        var result = Result.Failure(Error.Forbidden());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.Forbidden");
    }

    [Fact]
    public void Result_MatchAll_ShouldReceiveAllErrors()
    {
        var result = Result.Failure(new[]
        {
            Error.Validation("F1", "e1"),
            Error.Validation("F2", "e2")
        });

        var count = result.MatchAll(() => 0, errs => errs.Count);

        count.Should().Be(2);
    }

    [Fact]
    public void Result_AsResult_OnSuccess_ShouldWrapValue()
    {
        var result = Result.Success().AsResult("wrapped");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("wrapped");
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

    // --- ErrorType discriminator ---

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Internal)]
    public void Error_FactoryMethods_ShouldSetCorrectType(ErrorType expectedType)
    {
        Error error = expectedType switch
        {
            ErrorType.Validation   => Error.Validation("F", "v"),
            ErrorType.NotFound     => Error.NotFound("R", 1),
            ErrorType.Conflict     => Error.Conflict("C", "c"),
            ErrorType.Unauthorized => Error.Unauthorized(),
            ErrorType.Forbidden    => Error.Forbidden(),
            ErrorType.Internal     => Error.Internal("i"),
            _                      => throw new InvalidOperationException()
        };

        error.Type.Should().Be(expectedType);
    }

    [Fact]
    public void Error_None_ShouldHaveTypeNone()
    {
        Error.None.Type.Should().Be(ErrorType.None);
    }

    [Fact]
    public void Error_CustomConstructor_DefaultType_ShouldBeNone()
    {
        var error = new Error("Custom.Code", "desc");

        error.Type.Should().Be(ErrorType.None);
    }

    // --- Result<T>.Ensure ---

    [Fact]
    public void Ensure_OnSuccess_PredicateTrue_ShouldReturnOriginalResult()
    {
        var result = Result<int>.Success(10).Ensure(v => v > 0, Error.Validation("v", "must be positive"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Ensure_OnSuccess_PredicateFalse_ShouldReturnFailure()
    {
        var error = Error.Validation("v", "must be positive");
        var result = Result<int>.Success(-1).Ensure(v => v > 0, error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Ensure_OnFailure_ShouldPropagateOriginalError()
    {
        var originalError = Error.Internal("original");
        var result = Result<int>.Failure(originalError).Ensure(_ => false, Error.Validation("v", "irrelevant"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(originalError);
    }

    [Fact]
    public async Task EnsureAsync_OnSuccess_PredicateFalse_ShouldReturnFailure()
    {
        var error = Error.Validation("v", "must be even");
        var result = await Result<int>.Success(3).EnsureAsync(v => Task.FromResult(v % 2 == 0), error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    // --- Result<T>.Tap ---

    [Fact]
    public void Tap_OnSuccess_ShouldExecuteSideEffect_AndReturnOriginalResult()
    {
        var captured = 0;
        var result = Result<int>.Success(7).Tap(v => captured = v);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
        captured.Should().Be(7);
    }

    [Fact]
    public void Tap_OnFailure_ShouldNotExecuteSideEffect()
    {
        var called = false;
        Result<int>.Failure(Error.Internal("err")).Tap(_ => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapAsync_OnSuccess_ShouldExecuteSideEffect()
    {
        var captured = 0;
        await Result<int>.Success(9).TapAsync(v => { captured = v; return Task.CompletedTask; });

        captured.Should().Be(9);
    }

    // --- Result<T>.Bind(→Result) ---

    [Fact]
    public void Bind_ToVoidResult_OnSuccess_ShouldExecuteAndReturn()
    {
        var result = Result<int>.Success(5).Bind(v => v > 0 ? Result.Success() : Result.Failure(Error.Internal("neg")));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_ToVoidResult_OnSuccess_Failing_ShouldReturnFailure()
    {
        var result = Result<int>.Success(-1).Bind(_ => Result.Failure(Error.Internal("neg")));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Bind_ToVoidResult_OnFailure_ShouldPropagateError()
    {
        var original = Error.Internal("original");
        var result = Result<int>.Failure(original).Bind(_ => Result.Success());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(original);
    }

    // --- Result (non-generic) Tap / Bind ---

    [Fact]
    public void Result_Tap_OnSuccess_ShouldExecuteSideEffect()
    {
        var called = false;
        Result.Success().Tap(() => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void Result_Tap_OnFailure_ShouldNotExecuteSideEffect()
    {
        var called = false;
        Result.Failure(Error.Internal("err")).Tap(() => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void Result_Bind_OnSuccess_ShouldChain()
    {
        var result = Result.Success().Bind(() => Result.Failure(Error.Internal("chained")));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Internal.Error");
    }

    [Fact]
    public void Result_Bind_OnFailure_ShouldNotChain()
    {
        var called = false;
        var original = Error.Validation("x", "bad");
        var result = Result.Failure(original).Bind(() => { called = true; return Result.Success(); });

        called.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(original);
    }

    [Fact]
    public async Task Result_BindAsync_OnSuccess_ShouldChain()
    {
        var result = await Result.Success().BindAsync(() => Task.FromResult(Result.Failure(Error.Internal("async"))));

        result.IsFailure.Should().BeTrue();
    }

    // --- Result<T>.TapError / TapErrorAsync ---

    [Fact]
    public void TapError_OnFailure_ShouldExecuteSideEffect_AndReturnOriginalResult()
    {
        Error? captured = null;
        var result = Result<int>.Failure(Error.Internal("fail")).TapError(e => captured = e);

        result.IsFailure.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Code.Should().Be("Internal.Error");
    }

    [Fact]
    public void TapError_OnSuccess_ShouldNotExecuteSideEffect()
    {
        var called = false;
        Result<int>.Success(1).TapError((Error _) => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void TapError_AllErrors_OnFailure_ShouldReceiveAllErrors()
    {
        var errors = new[] { Error.Validation("A", "a"), Error.Validation("B", "b") };
        IReadOnlyCollection<Error>? captured = null;
        Result<int>.Failure(errors).TapError(errs => captured = errs);

        captured.Should().HaveCount(2);
    }

    [Fact]
    public async Task TapErrorAsync_OnFailure_ShouldExecuteSideEffect()
    {
        Error? captured = null;
        await Result<int>.Failure(Error.Forbidden()).TapErrorAsync(e => { captured = e; return Task.CompletedTask; });

        captured.Should().NotBeNull();
    }

    // --- Result<T>.MapError / MapErrors ---

    [Fact]
    public void MapError_OnFailure_ShouldTransformError()
    {
        var result = Result<int>.Failure(Error.NotFound("Order", 1))
            .MapError(_ => Error.Internal("translated"));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Internal);
    }

    [Fact]
    public void MapError_OnSuccess_ShouldReturnOriginalResult()
    {
        var result = Result<int>.Success(5).MapError(_ => Error.Internal("unused"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void MapErrors_OnFailure_ShouldTransformAllErrors()
    {
        var errors = new[] { Error.Validation("A", "a"), Error.Validation("B", "b") };
        var result = Result<int>.Failure(errors)
            .MapErrors(errs => errs.Select(e => Error.Internal(e.Description)));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().AllSatisfy(e => e.Type.Should().Be(ErrorType.Internal));
    }

    // --- Result (non-generic) TapError / TapErrorAsync ---

    [Fact]
    public void Result_TapError_OnFailure_ShouldExecuteSideEffect()
    {
        var called = false;
        Result.Failure(Error.Internal("err")).TapError((Error _) => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void Result_TapError_OnSuccess_ShouldNotExecuteSideEffect()
    {
        var called = false;
        Result.Success().TapError((Error _) => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public async Task Result_TapErrorAsync_OnFailure_ShouldExecuteSideEffect()
    {
        Error? captured = null;
        await Result.Failure(Error.Unauthorized()).TapErrorAsync(e => { captured = e; return Task.CompletedTask; });

        captured.Should().NotBeNull();
        captured!.Type.Should().Be(ErrorType.Unauthorized);
    }
}
