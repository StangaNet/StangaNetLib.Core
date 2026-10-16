using StangaNetLib.Core.Common;

namespace StangaNetLib.Core.Validators;

/// <summary>
/// Outcome of a domain-level validation.
/// Used by domain validators (not FluentValidation — that is an Application-layer concern).
/// </summary>
public sealed class ValidationResult
{
    /// <summary>True when validation passed with no errors.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when at least one validation error was recorded.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Structured errors, consistent with <see cref="Result{T}.Errors"/>.</summary>
    public IReadOnlyCollection<Error> Errors { get; }

    private ValidationResult(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>Creates a successful validation result with no errors.</summary>
    /// <returns>A <see cref="ValidationResult"/> indicating success.</returns>
    public static ValidationResult Success()
        => new(true, []);

    /// <summary>Creates a failure with a fully structured error.</summary>
    /// <param name="error">The structured error that caused validation to fail.</param>
    /// <returns>A <see cref="ValidationResult"/> carrying <paramref name="error"/>.</returns>
    public static ValidationResult Failure(Error error)
        => new(false, [error]);

    /// <summary>Creates a failure from multiple structured errors.</summary>
    /// <param name="errors">All structured errors that caused validation to fail.</param>
    /// <returns>A <see cref="ValidationResult"/> carrying all <paramref name="errors"/>.</returns>
    public static ValidationResult Failure(IEnumerable<Error> errors)
        => new(false, errors);

    /// <summary>Creates a validation failure from a plain description string (code defaults to "Validation").</summary>
    /// <param name="description">Human-readable description of the validation failure.</param>
    /// <returns>A <see cref="ValidationResult"/> with a single generic validation error.</returns>
    public static ValidationResult Failure(string description)
        => new(false, [new Error("Validation", description)]);

    /// <summary>Creates a validation failure from multiple plain description strings.</summary>
    /// <param name="descriptions">Human-readable descriptions of each validation failure.</param>
    /// <returns>A <see cref="ValidationResult"/> with one generic validation error per description.</returns>
    public static ValidationResult Failure(IEnumerable<string> descriptions)
        => new(false, descriptions.Select(d => new Error("Validation", d)));

    /// <summary>
    /// Combines multiple validation results into one. Returns <see cref="Success()"/> only when all
    /// inputs succeed; otherwise returns <see cref="Failure(IEnumerable{Error})"/> with every collected error.
    /// </summary>
    /// <param name="results">The validation results to combine.</param>
    /// <returns>A successful <see cref="ValidationResult"/> if all inputs succeeded; otherwise a failure with all errors.</returns>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        return errors.Count == 0 ? Success() : Failure((IEnumerable<Error>)errors);
    }

    /// <summary>Joins all error descriptions into a single semicolon-separated string.</summary>
    /// <returns>A semicolon-separated string of all error descriptions, or an empty string on success.</returns>
    public string GetErrorMessage()
        => string.Join("; ", Errors.Select(e => e.Description));

    /// <summary>Converts this ValidationResult to a <see cref="Result"/> for use in use cases.</summary>
    /// <returns>A successful <see cref="Result"/> on success, or a failed one carrying all errors.</returns>
    public Result ToResult()
        => IsSuccess ? Result.Success() : Result.Failure(Errors);

    /// <summary>Converts this ValidationResult to a typed <see cref="Result{T}"/> for use in use cases.</summary>
    /// <typeparam name="T">The value type of the target result.</typeparam>
    /// <returns>A failed <see cref="Result{T}"/> carrying all errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this result is successful — a value is required to construct a typed success.</exception>
    public Result<T> ToResult<T>()
        => IsSuccess
            ? throw new InvalidOperationException("Cannot convert a successful ValidationResult to Result<T> without a value.")
            : Result<T>.Failure(Errors);
}
