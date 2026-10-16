namespace StangaNetLib.Core.Exceptions;

/// <summary>
/// Base class for all domain exceptions. Derive project-specific exceptions from this class.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>Initialises a new domain exception with a descriptive message.</summary>
    /// <param name="message">A human-readable description of the domain rule violation.</param>
    protected DomainException(string message) : base(message) { }

    /// <summary>Initialises a new domain exception wrapping an inner exception.</summary>
    /// <param name="message">A human-readable description of the domain rule violation.</param>
    /// <param name="innerException">The underlying exception that caused this domain error.</param>
    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
