using Microsoft.Extensions.Options;

namespace StangaNetLib.Core.Validators;

/// <summary>
/// Base class for <see cref="IValidateOptions{TOptions}"/> implementations across StangaNetLib libraries.
/// Subclasses implement <see cref="ValidateCore"/> and append messages to the provided error collector;
/// this class owns the result assembly so concrete validators contain only domain-specific rules.
/// </summary>
/// <typeparam name="TOptions">The options type being validated.</typeparam>
public abstract class SettingsValidatorBase<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        var errors = new List<string>();
        ValidateCore(options, errors);
        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }

    /// <summary>
    /// Override to add domain-specific validation rules.
    /// Append violation messages to <paramref name="errors"/>; do not return early — all rules are always evaluated.
    /// </summary>
    /// <param name="options">The options instance to validate. Never null.</param>
    /// <param name="errors">Collector for validation failure messages.</param>
    protected abstract void ValidateCore(TOptions options, List<string> errors);
}
