using StangaNetLib.Core.Common;

namespace StangaNetLib.Core.Validators;

/// <summary>
/// Outcome of a domain-level validation.
/// Used by domain validators (not FluentValidation — that is an Application-layer concern).
/// </summary>
public sealed class ValidationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyCollection<string> Errors { get; }

    private ValidationResult(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList().AsReadOnly();
    }

    public static ValidationResult Success()
        => new(true, []);

    public static ValidationResult Failure(string error)
        => new(false, [error]);

    public static ValidationResult Failure(IEnumerable<string> errors)
        => new(false, errors);

    /// <summary>Joins all error messages into a single semicolon-separated string.</summary>
    public string GetErrorMessage()
        => string.Join("; ", Errors);

    /// <summary>Converts this ValidationResult to a <see cref="Result"/> for use in use cases.</summary>
    public Result ToResult()
        => IsSuccess
            ? Result.Success()
            : Result.Failure(Errors.Select(e => new Error("Validation", e)));

    /// <summary>Converts this ValidationResult to a typed <see cref="Result{T}"/> for use in use cases.</summary>
    public Result<T> ToResult<T>()
        => IsSuccess
            ? throw new InvalidOperationException("Cannot convert a successful ValidationResult to Result<T> without a value.")
            : Result<T>.Failure(Errors.Select(e => new Error("Validation", e)));
}
