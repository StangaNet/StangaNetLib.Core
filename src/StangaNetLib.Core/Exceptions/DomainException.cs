namespace StangaNetLib.Core.Exceptions;

/// <summary>
/// Base class for all domain exceptions. Derive project-specific exceptions from this class.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
