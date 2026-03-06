namespace StangaNetLib.Core.Common;

/// <summary>
/// Structured error with a machine-readable Code and a human-readable Description.
/// </summary>
public sealed record Error(string Code, string Description)
{
    /// <summary>Represents the absence of an error (used internally by successful Results).</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>Creates a validation error for a specific field.</summary>
    public static Error Validation(string field, string description)
        => new($"Validation.{field}", description);

    /// <summary>Creates a not-found error for a named resource.</summary>
    public static Error NotFound(string resource, object id)
        => new($"{resource}.NotFound", $"{resource} with id '{id}' was not found.");

    /// <summary>Creates a conflict error (e.g. duplicate entity).</summary>
    public static Error Conflict(string code, string description)
        => new(code, description);

    /// <summary>Creates an unauthorized error.</summary>
    public static Error Unauthorized(string description = "Unauthorized access.")
        => new("Auth.Unauthorized", description);

    /// <summary>Creates a forbidden error.</summary>
    public static Error Forbidden(string description = "Access forbidden.")
        => new("Auth.Forbidden", description);

    /// <summary>Creates a generic internal error.</summary>
    public static Error Internal(string description)
        => new("Internal.Error", description);

    public override string ToString() => $"[{Code}] {Description}";
}

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// Use <see cref="Result{T}.Success"/> or <see cref="Result{T}.Failure(Error)"/> factory methods.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    /// <summary>The result value. Only access when <see cref="IsSuccess"/> is true.</summary>
    public T? Value { get; }

    /// <summary>The primary error. <see cref="Error.None"/> when successful.</summary>
    public Error Error { get; }

    /// <summary>All errors when multiple validation failures occur.</summary>
    public IReadOnlyCollection<Error> Errors { get; }

    private Result(bool isSuccess, T? value, Error error, IEnumerable<Error>? errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = (errors ?? Enumerable.Empty<Error>()).ToList().AsReadOnly();
    }

    // --- Factory methods ---

    public static Result<T> Success(T value)
        => new(true, value, Error.None, null);

    public static Result<T> Failure(Error error)
        => new(false, default, error, [error]);

    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        return new(false, default, list.FirstOrDefault() ?? Error.None, list);
    }

    /// <summary>Convenience overload for string-based errors (migration compatibility).</summary>
    public static Result<T> Failure(string errorDescription)
        => Failure(new Error("Error", errorDescription));

    // --- Combinators ---

    /// <summary>
    /// Executes <paramref name="onSuccess"/> if successful, <paramref name="onFailure"/> otherwise.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error);

    /// <summary>Transforms the value if successful; propagates failure otherwise.</summary>
    public Result<TNext> Map<TNext>(Func<T, TNext> map)
        => IsSuccess ? Result<TNext>.Success(map(Value!)) : Result<TNext>.Failure(Errors);

    /// <summary>Asynchronously transforms the value if successful; propagates failure otherwise.</summary>
    public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> map)
        => IsSuccess ? Result<TNext>.Success(await map(Value!)) : Result<TNext>.Failure(Errors);

    /// <summary>Chains another Result-returning operation if successful.</summary>
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind)
        => IsSuccess ? bind(Value!) : Result<TNext>.Failure(Errors);

    /// <summary>Asynchronously chains another Result-returning operation if successful.</summary>
    public async Task<Result<TNext>> BindAsync<TNext>(Func<T, Task<Result<TNext>>> bind)
        => IsSuccess ? await bind(Value!) : Result<TNext>.Failure(Errors);

    // --- Implicit conversions ---

    /// <summary>Allows returning a value directly where Result&lt;T&gt; is expected.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Allows returning an Error directly where Result&lt;T&gt; is expected.</summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public IReadOnlyCollection<Error> Errors { get; }

    private Result(bool isSuccess, Error error, IEnumerable<Error>? errors)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = (errors ?? Enumerable.Empty<Error>()).ToList().AsReadOnly();
    }

    public static Result Success()
        => new(true, Error.None, null);

    public static Result Failure(Error error)
        => new(false, error, [error]);

    public static Result Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        return new(false, list.FirstOrDefault() ?? Error.None, list);
    }

    public static Result Failure(string errorDescription)
        => Failure(new Error("Error", errorDescription));

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);

    /// <summary>Wraps this Result into a typed Result with a given value on success.</summary>
    public Result<T> AsResult<T>(T value)
        => IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(Errors);

    public static implicit operator Result(Error error) => Failure(error);
}
