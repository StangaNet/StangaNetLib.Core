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
    /// <returns>The sequence of values that jointly identify this value object.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var c in GetEqualityComponents())
            hc.Add(c);
        return hc.ToHashCode();
    }

    /// <summary>Returns true when both value objects are equal by components, or both are null.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left is null && right is null || (left is not null && left.Equals(right));

    /// <summary>Returns true when the two value objects differ by at least one component.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}
