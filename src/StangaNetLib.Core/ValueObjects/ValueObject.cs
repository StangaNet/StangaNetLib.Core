namespace StangaNetLib.Core.ValueObjects;

/// <summary>
/// Base class for DDD Value Objects.
/// Equality is based on the components returned by <see cref="GetEqualityComponents"/>,
/// not on object reference.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Returns all components that define equality for this value object.
    /// Example: for a Money value object, return [Amount, Currency].
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate(HashCode.Combine);

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left is null && right is null || (left is not null && left.Equals(right));

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
