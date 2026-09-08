namespace StangaNetLib.Core.Common;

/// <summary>
/// Discriminates the category of an <see cref="Error"/> without requiring string parsing.
/// Consumers can switch on this value to map errors to HTTP status codes or other outcomes.
/// </summary>
public enum ErrorType
{
    /// <summary>Used only by <see cref="Error.None"/> — not a real failure.</summary>
    None,
    /// <summary>One or more input values did not pass domain validation.</summary>
    Validation,
    /// <summary>A requested resource was not found.</summary>
    NotFound,
    /// <summary>The operation conflicts with existing state (e.g. duplicate).</summary>
    Conflict,
    /// <summary>The caller is not authenticated.</summary>
    Unauthorized,
    /// <summary>The caller is authenticated but does not have permission.</summary>
    Forbidden,
    /// <summary>An unexpected internal error occurred.</summary>
    Internal,
}

/// <summary>
/// Structured error with a machine-readable Code and a human-readable Description.
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.None)
{
    /// <summary>Represents the absence of an error (used internally by successful Results).</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>Creates a validation error for a specific field.</summary>
    /// <param name="field">The field name that failed validation. Becomes part of the error code as <c>Validation.{field}</c>.</param>
    /// <param name="description">Human-readable description of the validation failure.</param>
    /// <returns>An <see cref="Error"/> with code <c>Validation.{field}</c> and type <see cref="ErrorType.Validation"/>.</returns>
    public static Error Validation(string field, string description)
        => new($"Validation.{field}", description, ErrorType.Validation);

    /// <summary>Creates a not-found error for a named resource.</summary>
    /// <param name="resource">The resource type that was not found (e.g. "User", "Order").</param>
    /// <param name="id">The identifier that was looked up.</param>
    /// <returns>An <see cref="Error"/> with code <c>{resource}.NotFound</c> and type <see cref="ErrorType.NotFound"/>.</returns>
    public static Error NotFound(string resource, object id)
        => new($"{resource}.NotFound", $"{resource} with id '{id}' was not found.", ErrorType.NotFound);

    /// <summary>Creates a conflict error (e.g. duplicate entity).</summary>
    /// <param name="code">Machine-readable error code.</param>
    /// <param name="description">Human-readable description of the conflict.</param>
    /// <returns>An <see cref="Error"/> with the given code and type <see cref="ErrorType.Conflict"/>.</returns>
    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    /// <summary>Creates an unauthorized error.</summary>
    /// <param name="description">Human-readable reason. Defaults to "Unauthorized access."</param>
    /// <returns>An <see cref="Error"/> with code <c>Auth.Unauthorized</c> and type <see cref="ErrorType.Unauthorized"/>.</returns>
    public static Error Unauthorized(string description = "Unauthorized access.")
        => new("Auth.Unauthorized", description, ErrorType.Unauthorized);

    /// <summary>Creates a forbidden error.</summary>
    /// <param name="description">Human-readable reason. Defaults to "Access forbidden."</param>
    /// <returns>An <see cref="Error"/> with code <c>Auth.Forbidden</c> and type <see cref="ErrorType.Forbidden"/>.</returns>
    public static Error Forbidden(string description = "Access forbidden.")
        => new("Auth.Forbidden", description, ErrorType.Forbidden);

    /// <summary>Creates a generic internal error.</summary>
    /// <param name="description">Human-readable description of the internal error.</param>
    /// <returns>An <see cref="Error"/> with code <c>Internal.Error</c> and type <see cref="ErrorType.Internal"/>.</returns>
    public static Error Internal(string description)
        => new("Internal.Error", description, ErrorType.Internal);

    /// <summary>Returns the error formatted as <c>[Code] Description</c>.</summary>
    /// <returns>A string in the form <c>[Code] Description</c>.</returns>
    public override string ToString() => $"[{Code}] {Description}";
}

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// Use <see cref="Result{T}.Success"/> or <see cref="Result{T}.Failure(Error)"/> factory methods.
/// </summary>
public sealed class Result<T>
{
    /// <summary>True when the operation completed without errors.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when the operation produced one or more errors.</summary>
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
        Errors = errors is null ? [] : errors.ToList().AsReadOnly();
    }

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <param name="value">The value produced by the operation.</param>
    /// <returns>A successful <see cref="Result{T}"/> wrapping <paramref name="value"/>.</returns>
    public static Result<T> Success(T value)
        => new(true, value, Error.None, null);

    /// <summary>Creates a failed result with a single error.</summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying <paramref name="error"/>.</returns>
    public static Result<T> Failure(Error error)
        => new(false, default, error, [error]);

    /// <summary>Creates a failed result from multiple errors. The first error becomes <see cref="Error"/>.</summary>
    /// <param name="errors">All errors that caused the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> carrying all <paramref name="errors"/>.</returns>
    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        return new(false, default, list.FirstOrDefault() ?? Error.None, list);
    }

    /// <summary>Convenience overload for string-based errors (migration compatibility).</summary>
    /// <param name="errorDescription">Plain-text description; code defaults to "Error".</param>
    /// <returns>A failed <see cref="Result{T}"/> with a generic error code.</returns>
    public static Result<T> Failure(string errorDescription)
        => Failure(new Error("Error", errorDescription));

    /// <summary>Executes <paramref name="onSuccess"/> if successful, <paramref name="onFailure"/> otherwise.</summary>
    /// <typeparam name="TResult">The return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked with the value on success.</param>
    /// <param name="onFailure">Invoked with the primary error on failure.</param>
    /// <returns>The value returned by whichever branch was executed.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error);

    /// <summary>
    /// Executes <paramref name="onSuccess"/> if successful, <paramref name="onFailure"/> otherwise.
    /// The failure branch receives the full error collection — use when multiple validation errors are present.
    /// Named <c>MatchAll</c> to avoid lambda ambiguity with the single-error <see cref="Match{TResult}(Func{T,TResult},Func{Error,TResult})"/> overload.
    /// </summary>
    /// <typeparam name="TResult">The return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked with the value on success.</param>
    /// <param name="onFailure">Invoked with all errors on failure.</param>
    /// <returns>The value returned by whichever branch was executed.</returns>
    public TResult MatchAll<TResult>(Func<T, TResult> onSuccess, Func<IReadOnlyCollection<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Errors);

    /// <summary>Transforms the value if successful; propagates failure otherwise.</summary>
    /// <typeparam name="TNext">The type of the transformed value.</typeparam>
    /// <param name="map">Projection applied to the success value.</param>
    /// <returns>A successful <see cref="Result{TNext}"/> with the mapped value, or the original failure.</returns>
    public Result<TNext> Map<TNext>(Func<T, TNext> map)
        => IsSuccess ? Result<TNext>.Success(map(Value!)) : Result<TNext>.Failure(Errors);

    /// <summary>Asynchronously transforms the value if successful; propagates failure otherwise.</summary>
    /// <typeparam name="TNext">The type of the transformed value.</typeparam>
    /// <param name="map">Async projection applied to the success value.</param>
    /// <returns>A successful <see cref="Result{TNext}"/> with the awaited mapped value, or the original failure.</returns>
    public async Task<Result<TNext>> MapAsync<TNext>(Func<T, Task<TNext>> map)
        => IsSuccess ? Result<TNext>.Success(await map(Value!)) : Result<TNext>.Failure(Errors);

    /// <summary>Chains another Result-returning operation if successful.</summary>
    /// <typeparam name="TNext">The value type returned by the next operation.</typeparam>
    /// <param name="bind">Operation to invoke with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure propagated.</returns>
    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind)
        => IsSuccess ? bind(Value!) : Result<TNext>.Failure(Errors);

    /// <summary>Asynchronously chains another Result-returning operation if successful.</summary>
    /// <typeparam name="TNext">The value type returned by the next operation.</typeparam>
    /// <param name="bind">Async operation to invoke with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure propagated.</returns>
    public async Task<Result<TNext>> BindAsync<TNext>(Func<T, Task<Result<TNext>>> bind)
        => IsSuccess ? await bind(Value!) : Result<TNext>.Failure(Errors);

    /// <summary>Chains a void Result-returning operation if successful.</summary>
    /// <param name="bind">Void operation to invoke with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure propagated as a void <see cref="Result"/>.</returns>
    public Result Bind(Func<T, Result> bind)
        => IsSuccess ? bind(Value!) : Result.Failure(Errors);

    /// <summary>Asynchronously chains a void Result-returning operation if successful.</summary>
    /// <param name="bind">Async void operation to invoke with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure propagated as a void <see cref="Result"/>.</returns>
    public async Task<Result> BindAsync(Func<T, Task<Result>> bind)
        => IsSuccess ? await bind(Value!) : Result.Failure(Errors);

    /// <summary>Returns a failure if the predicate is false on the success value; otherwise propagates success unchanged.</summary>
    /// <param name="predicate">Condition the success value must satisfy.</param>
    /// <param name="error">Error returned when <paramref name="predicate"/> returns false.</param>
    /// <returns>The original result when successful and the predicate holds; otherwise a failure with <paramref name="error"/>.</returns>
    public Result<T> Ensure(Func<T, bool> predicate, Error error)
        => IsSuccess && !predicate(Value!) ? Failure(error) : this;

    /// <summary>Asynchronously evaluates the predicate on the success value; returns a failure if it returns false.</summary>
    /// <param name="predicate">Async condition the success value must satisfy.</param>
    /// <param name="error">Error returned when <paramref name="predicate"/> returns false.</param>
    /// <returns>The original result when successful and the predicate holds; otherwise a failure with <paramref name="error"/>.</returns>
    public async Task<Result<T>> EnsureAsync(Func<T, Task<bool>> predicate, Error error)
        => IsSuccess && !await predicate(Value!) ? Failure(error) : this;

    /// <summary>Executes a side effect on success without altering the result. Useful for logging or telemetry.</summary>
    /// <param name="action">Side effect to execute with the success value.</param>
    /// <returns>The original result, unchanged.</returns>
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess) action(Value!);
        return this;
    }

    /// <summary>Asynchronously executes a side effect on success without altering the result.</summary>
    /// <param name="action">Async side effect to execute with the success value.</param>
    /// <returns>The original result, unchanged.</returns>
    public async Task<Result<T>> TapAsync(Func<T, Task> action)
    {
        if (IsSuccess) await action(Value!);
        return this;
    }

    /// <summary>Executes a side effect on failure without altering the result. Useful for logging errors.</summary>
    /// <param name="action">Side effect to execute with the primary error.</param>
    /// <returns>The original result, unchanged.</returns>
    public Result<T> TapError(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    /// <summary>Executes a side effect on failure, receiving all errors. Useful when multiple validation errors must be logged.</summary>
    /// <param name="action">Side effect to execute with the full error collection.</param>
    /// <returns>The original result, unchanged.</returns>
    public Result<T> TapError(Action<IReadOnlyCollection<Error>> action)
    {
        if (IsFailure) action(Errors);
        return this;
    }

    /// <summary>Asynchronously executes a side effect on failure without altering the result.</summary>
    /// <param name="action">Async side effect to execute with the primary error.</param>
    /// <returns>The original result, unchanged.</returns>
    public async Task<Result<T>> TapErrorAsync(Func<Error, Task> action)
    {
        if (IsFailure) await action(Error);
        return this;
    }

    /// <summary>Transforms the primary error if the result is a failure; propagates success unchanged.</summary>
    /// <param name="mapError">Projection applied to the primary error.</param>
    /// <returns>A failed result with the mapped error, or the original success.</returns>
    public Result<T> MapError(Func<Error, Error> mapError)
        => IsFailure ? Failure(mapError(Error)) : this;

    /// <summary>Transforms all errors if the result is a failure; propagates success unchanged.</summary>
    /// <param name="mapErrors">Projection applied to the full error collection.</param>
    /// <returns>A failed result with the mapped errors, or the original success.</returns>
    public Result<T> MapErrors(Func<IReadOnlyCollection<Error>, IEnumerable<Error>> mapErrors)
        => IsFailure ? Failure(mapErrors(Errors)) : this;

    /// <summary>Allows returning a value directly where Result&lt;T&gt; is expected.</summary>
    public static implicit operator Result<T>(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create a successful Result with a null value. Return a Failure instead.");
        return Success(value);
    }

    /// <summary>Allows returning an Error directly where Result&lt;T&gt; is expected.</summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public sealed class Result
{
    /// <summary>True when the operation completed without errors.</summary>
    public bool IsSuccess { get; }

    /// <summary>True when the operation produced one or more errors.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>The primary error. <see cref="Error.None"/> when successful.</summary>
    public Error Error { get; }

    /// <summary>All errors when multiple validation failures occur.</summary>
    public IReadOnlyCollection<Error> Errors { get; }

    private Result(bool isSuccess, Error error, IEnumerable<Error>? errors)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors is null ? [] : errors.ToList().AsReadOnly();
    }

    /// <summary>Creates a successful void result.</summary>
    /// <returns>A successful <see cref="Result"/> with no errors.</returns>
    public static Result Success()
        => new(true, Error.None, null);

    /// <summary>Creates a failed result with a single error.</summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying <paramref name="error"/>.</returns>
    public static Result Failure(Error error)
        => new(false, error, [error]);

    /// <summary>Creates a failed result from multiple errors. The first error becomes <see cref="Error"/>.</summary>
    /// <param name="errors">All errors that caused the failure.</param>
    /// <returns>A failed <see cref="Result"/> carrying all <paramref name="errors"/>.</returns>
    public static Result Failure(IEnumerable<Error> errors)
    {
        var list = errors.ToList();
        return new(false, list.FirstOrDefault() ?? Error.None, list);
    }

    /// <summary>Convenience overload for string-based errors (migration compatibility).</summary>
    /// <param name="errorDescription">Plain-text description; code defaults to "Error".</param>
    /// <returns>A failed <see cref="Result"/> with a generic error code.</returns>
    public static Result Failure(string errorDescription)
        => Failure(new Error("Error", errorDescription));

    /// <summary>Executes <paramref name="onSuccess"/> if successful, <paramref name="onFailure"/> otherwise.</summary>
    /// <typeparam name="TResult">The return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked on success.</param>
    /// <param name="onFailure">Invoked with the primary error on failure.</param>
    /// <returns>The value returned by whichever branch was executed.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error);

    /// <summary>
    /// Executes <paramref name="onSuccess"/> if successful, <paramref name="onFailure"/> otherwise.
    /// The failure branch receives the full error collection — use when multiple validation errors are present.
    /// Named <c>MatchAll</c> to avoid lambda ambiguity with the single-error <see cref="Match{TResult}(Func{TResult},Func{Error,TResult})"/> overload.
    /// </summary>
    /// <typeparam name="TResult">The return type of both branches.</typeparam>
    /// <param name="onSuccess">Invoked on success.</param>
    /// <param name="onFailure">Invoked with all errors on failure.</param>
    /// <returns>The value returned by whichever branch was executed.</returns>
    public TResult MatchAll<TResult>(Func<TResult> onSuccess, Func<IReadOnlyCollection<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Errors);

    /// <summary>Wraps this Result into a typed Result with a given value on success.</summary>
    /// <typeparam name="T">The value type of the returned result.</typeparam>
    /// <param name="value">The value to wrap on success.</param>
    /// <returns>A successful <see cref="Result{T}"/> with <paramref name="value"/>, or a failed one carrying this result's errors.</returns>
    public Result<T> AsResult<T>(T value)
        => IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(Errors);

    /// <summary>Allows returning an Error directly where Result is expected.</summary>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>Executes a side effect on success without altering the result. Useful for logging or telemetry.</summary>
    /// <param name="action">Side effect to execute on success.</param>
    /// <returns>The original result, unchanged.</returns>
    public Result Tap(Action action)
    {
        if (IsSuccess) action();
        return this;
    }

    /// <summary>Asynchronously executes a side effect on success without altering the result.</summary>
    /// <param name="action">Async side effect to execute on success.</param>
    /// <returns>The original result, unchanged.</returns>
    public async Task<Result> TapAsync(Func<Task> action)
    {
        if (IsSuccess) await action();
        return this;
    }

    /// <summary>Executes a side effect on failure without altering the result. Useful for logging errors.</summary>
    /// <param name="action">Side effect to execute with the primary error.</param>
    /// <returns>The original result, unchanged.</returns>
    public Result TapError(Action<Error> action)
    {
        if (IsFailure) action(Error);
        return this;
    }

    /// <summary>Executes a side effect on failure, receiving all errors.</summary>
    /// <param name="action">Side effect to execute with the full error collection.</param>
    /// <returns>The original result, unchanged.</returns>
    public Result TapError(Action<IReadOnlyCollection<Error>> action)
    {
        if (IsFailure) action(Errors);
        return this;
    }

    /// <summary>Asynchronously executes a side effect on failure without altering the result.</summary>
    /// <param name="action">Async side effect to execute with the primary error.</param>
    /// <returns>The original result, unchanged.</returns>
    public async Task<Result> TapErrorAsync(Func<Error, Task> action)
    {
        if (IsFailure) await action(Error);
        return this;
    }

    /// <summary>Chains another void Result-returning operation if successful.</summary>
    /// <param name="bind">Operation to invoke on success.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure if already failed.</returns>
    public Result Bind(Func<Result> bind)
        => IsSuccess ? bind() : this;

    /// <summary>Asynchronously chains another void Result-returning operation if successful.</summary>
    /// <param name="bind">Async operation to invoke on success.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure if already failed.</returns>
    public async Task<Result> BindAsync(Func<Task<Result>> bind)
        => IsSuccess ? await bind() : this;

    /// <summary>
    /// Combines multiple void results into one. Returns <see cref="Success()"/> only when all
    /// inputs succeed; otherwise returns <see cref="Failure(IEnumerable{Error})"/> with every
    /// collected error.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful <see cref="Result"/> if all inputs succeeded; otherwise a failure with all errors.</returns>
    public static Result Combine(params Result[] results)
    {
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        return errors.Count == 0 ? Success() : Failure(errors);
    }

    /// <summary>
    /// Combines multiple typed results into a single <c>Result&lt;IReadOnlyList&lt;T&gt;&gt;</c>.
    /// Returns all values on success, or all collected errors on any failure.
    /// </summary>
    /// <typeparam name="T">The value type of the input results.</typeparam>
    /// <param name="results">The typed results to combine.</param>
    /// <returns>A successful result with all values in order, or a failure with all collected errors.</returns>
    public static Result<IReadOnlyList<T>> Combine<T>(params Result<T>[] results)
    {
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
        if (errors.Count > 0)
            return Result<IReadOnlyList<T>>.Failure(errors);
        return Result<IReadOnlyList<T>>.Success(results.Select(r => r.Value!).ToList().AsReadOnly());
    }
}
