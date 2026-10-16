using System.Numerics;
using System.Text.RegularExpressions;

namespace StangaNetLib.Core.Guards;

/// <summary>
/// Provides guard clauses for defensive programming.
/// Usage: <c>Guard.Against.Null(value, nameof(value));</c>
/// </summary>
public static class Guard
{
    /// <summary>Entry point for all guard clauses.</summary>
    public static class Against
    {
        /// <summary>Throws <see cref="ArgumentNullException"/> if <paramref name="value"/> is null.</summary>
        /// <typeparam name="T">Reference type being checked.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when not null.</returns>
        public static T Null<T>(T? value, string parameterName) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(parameterName, $"'{parameterName}' must not be null.");
            return value;
        }

        /// <summary>Throws <see cref="ArgumentNullException"/> if the nullable struct <paramref name="value"/> has no value.</summary>
        /// <typeparam name="T">Struct type being checked.</typeparam>
        /// <param name="value">The nullable struct to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns>The unwrapped value when present.</returns>
        public static T Null<T>(T? value, string parameterName) where T : struct
        {
            if (!value.HasValue)
                throw new ArgumentNullException(parameterName, $"'{parameterName}' must not be null.");
            return value.Value;
        }

        /// <summary>Throws <see cref="ArgumentException"/> if the string is null or empty.</summary>
        /// <param name="value">The string to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when not null or empty.</returns>
        public static string NullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"'{parameterName}' must not be null or empty.", parameterName);
            return value;
        }

        /// <summary>Throws <see cref="ArgumentException"/> if the string is null, empty, or whitespace-only.</summary>
        /// <param name="value">The string to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when it contains non-whitespace characters.</returns>
        public static string NullOrWhiteSpace(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"'{parameterName}' must not be null or whitespace.", parameterName);
            return value;
        }

        /// <summary>Throws <see cref="ArgumentException"/> if the value equals the default for its type (e.g. 0 for int, Guid.Empty for Guid).</summary>
        /// <typeparam name="T">Struct type being checked.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when not equal to <c>default(T)</c>.</returns>
        public static T Default<T>(T value, string parameterName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new ArgumentException($"'{parameterName}' must not be the default value.", parameterName);
            return value;
        }

        /// <summary>Throws <see cref="ArgumentException"/> if <paramref name="value"/> is an empty Guid.</summary>
        /// <param name="value">The Guid to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when not <see cref="Guid.Empty"/>.</returns>
        public static Guid EmptyGuid(Guid value, string parameterName)
        {
            if (value == Guid.Empty)
                throw new ArgumentException($"'{parameterName}' must not be an empty Guid.", parameterName);
            return value;
        }

        /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative. Supports all numeric types.</summary>
        /// <typeparam name="T">Numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when zero or positive.</returns>
        public static T Negative<T>(T value, string parameterName) where T : INumber<T>
        {
            if (value < T.Zero)
                throw new ArgumentOutOfRangeException(parameterName, $"'{parameterName}' must not be negative.");
            return value;
        }

        /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero or negative. Supports all numeric types.</summary>
        /// <typeparam name="T">Numeric type implementing <see cref="INumber{T}"/>.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when strictly positive.</returns>
        public static T NegativeOrZero<T>(T value, string parameterName) where T : INumber<T>
        {
            if (value <= T.Zero)
                throw new ArgumentOutOfRangeException(parameterName, $"'{parameterName}' must be greater than zero.");
            return value;
        }

        /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is outside [<paramref name="min"/>, <paramref name="max"/>].</summary>
        /// <typeparam name="T">Comparable type.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Inclusive upper bound.</param>
        /// <returns><paramref name="value"/> when within the inclusive range.</returns>
        public static T OutOfRange<T>(T value, string parameterName, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(parameterName,
                    $"'{parameterName}' must be between {min} and {max}. Actual: {value}.");
            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the collection is null or contains no elements.
        /// Materializes the sequence to avoid double-enumeration of lazy sources.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="value">The collection to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns>A materialized read-only list when the collection has at least one element.</returns>
        public static IReadOnlyList<T> NullOrEmpty<T>(IEnumerable<T>? value, string parameterName)
        {
            var list = value?.ToList();
            if (list is null || list.Count == 0)
                throw new ArgumentException($"'{parameterName}' must not be null or empty.", parameterName);
            return list.AsReadOnly();
        }

        /// <summary>Throws <see cref="ArgumentException"/> if the string exceeds <paramref name="maxLength"/> characters.</summary>
        /// <param name="value">The string to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <param name="maxLength">Maximum allowed number of characters (inclusive).</param>
        /// <returns><paramref name="value"/> when within the length limit.</returns>
        public static string TooLong(string value, string parameterName, int maxLength)
        {
            ArgumentNullException.ThrowIfNull(value, parameterName);
            if (value.Length > maxLength)
                throw new ArgumentException(
                    $"'{parameterName}' must not exceed {maxLength} characters. Actual length: {value.Length}.",
                    parameterName);
            return value;
        }

        /// <summary>Throws <see cref="ArgumentException"/> if the string is shorter than <paramref name="minLength"/> characters.</summary>
        /// <param name="value">The string to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <param name="minLength">Minimum required number of characters (inclusive).</param>
        /// <returns><paramref name="value"/> when long enough.</returns>
        public static string TooShort(string value, string parameterName, int minLength)
        {
            ArgumentNullException.ThrowIfNull(value, parameterName);
            if (value.Length < minLength)
                throw new ArgumentException(
                    $"'{parameterName}' must be at least {minLength} characters. Actual length: {value.Length}.",
                    parameterName);
            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the string does not match <paramref name="pattern"/>.
        /// Use a pre-compiled <see cref="Regex"/> (e.g. a static readonly field) to avoid recompilation on every call.
        /// </summary>
        /// <param name="value">The string to check. Must not be null or whitespace.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <param name="pattern">The compiled regex the value must match.</param>
        /// <returns><paramref name="value"/> when it matches <paramref name="pattern"/>.</returns>
        public static string Matches(string value, string parameterName, Regex pattern)
        {
            NullOrWhiteSpace(value, parameterName);
            if (!pattern.IsMatch(value))
                throw new ArgumentException(
                    $"'{parameterName}' does not match the required pattern.", parameterName);
            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not a defined member
        /// of <typeparamref name="TEnum"/>. Useful for validating enum parameters arriving from external input.
        /// </summary>
        /// <typeparam name="TEnum">The enum type to validate against.</typeparam>
        /// <param name="value">The enum value to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <returns><paramref name="value"/> when it is a defined member of <typeparamref name="TEnum"/>.</returns>
        public static TEnum InvalidEnum<TEnum>(TEnum value, string parameterName) where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"'{parameterName}' value '{value}' is not a valid member of '{typeof(TEnum).Name}'.");
            return value;
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if the string length is outside [<paramref name="minLength"/>, <paramref name="maxLength"/>].
        /// Combines <c>TooShort</c> and <c>TooLong</c> into a single check.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="parameterName">Name of the parameter, used in the exception message.</param>
        /// <param name="minLength">Minimum required number of characters (inclusive).</param>
        /// <param name="maxLength">Maximum allowed number of characters (inclusive).</param>
        /// <returns><paramref name="value"/> when its length is within the specified bounds.</returns>
        public static string LengthBetween(string value, string parameterName, int minLength, int maxLength)
        {
            ArgumentNullException.ThrowIfNull(value, parameterName);
            if (value.Length < minLength || value.Length > maxLength)
                throw new ArgumentException(
                    $"'{parameterName}' length must be between {minLength} and {maxLength}. Actual length: {value.Length}.",
                    parameterName);
            return value;
        }
    }
}
