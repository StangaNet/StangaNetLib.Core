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
        public static T Null<T>(T? value, string parameterName) where T : class
        {
            if (value is null)
                throw new ArgumentNullException(parameterName, $"'{parameterName}' must not be null.");
            return value;
        }

        /// <summary>Throws <see cref="ArgumentNullException"/> if <paramref name="value"/> (struct) is null.</summary>
        public static T Null<T>(T? value, string parameterName) where T : struct
        {
            if (!value.HasValue)
                throw new ArgumentNullException(parameterName, $"'{parameterName}' must not be null.");
            return value.Value;
        }

        /// <summary>Throws if the string is null or empty.</summary>
        public static string NullOrEmpty(string? value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"'{parameterName}' must not be null or empty.", parameterName);
            return value;
        }

        /// <summary>Throws if the string is null, empty, or whitespace-only.</summary>
        public static string NullOrWhiteSpace(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"'{parameterName}' must not be null or whitespace.", parameterName);
            return value;
        }

        /// <summary>Throws if the value equals the default for its type (e.g. 0 for int, Guid.Empty for Guid).</summary>
        public static T Default<T>(T value, string parameterName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new ArgumentException($"'{parameterName}' must not be the default value.", parameterName);
            return value;
        }

        /// <summary>Throws if <paramref name="value"/> is an empty Guid.</summary>
        public static Guid EmptyGuid(Guid value, string parameterName)
        {
            if (value == Guid.Empty)
                throw new ArgumentException($"'{parameterName}' must not be an empty Guid.", parameterName);
            return value;
        }

        /// <summary>Throws if <paramref name="value"/> is negative.</summary>
        public static int Negative(int value, string parameterName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(parameterName, $"'{parameterName}' must not be negative.");
            return value;
        }

        /// <summary>Throws if <paramref name="value"/> is zero or negative.</summary>
        public static int NegativeOrZero(int value, string parameterName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(parameterName, $"'{parameterName}' must be greater than zero.");
            return value;
        }

        /// <summary>Throws if <paramref name="value"/> is outside [<paramref name="min"/>, <paramref name="max"/>].</summary>
        public static T OutOfRange<T>(T value, string parameterName, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(parameterName,
                    $"'{parameterName}' must be between {min} and {max}. Actual: {value}.");
            return value;
        }

        /// <summary>Throws if the collection is null or contains no elements.</summary>
        public static IEnumerable<T> NullOrEmpty<T>(IEnumerable<T>? value, string parameterName)
        {
            if (value is null || !value.Any())
                throw new ArgumentException($"'{parameterName}' must not be null or empty.", parameterName);
            return value;
        }

        /// <summary>Throws if the string exceeds <paramref name="maxLength"/> characters.</summary>
        public static string TooLong(string value, string parameterName, int maxLength)
        {
            if (value.Length > maxLength)
                throw new ArgumentException(
                    $"'{parameterName}' must not exceed {maxLength} characters. Actual length: {value.Length}.",
                    parameterName);
            return value;
        }

        /// <summary>Throws if the string is shorter than <paramref name="minLength"/> characters.</summary>
        public static string TooShort(string value, string parameterName, int minLength)
        {
            if (value.Length < minLength)
                throw new ArgumentException(
                    $"'{parameterName}' must be at least {minLength} characters. Actual length: {value.Length}.",
                    parameterName);
            return value;
        }
    }
}
