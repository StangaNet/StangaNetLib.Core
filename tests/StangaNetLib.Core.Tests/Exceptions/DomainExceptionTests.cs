using FluentAssertions;
using StangaNetLib.Core.Exceptions;

namespace StangaNetLib.Core.Tests.Exceptions;

public class DomainExceptionTests
{
    private sealed class SampleDomainException : DomainException
    {
        public SampleDomainException(string message) : base(message) { }

        public SampleDomainException(string message, Exception inner)
            : base(message, inner) { }
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var ex = new SampleDomainException("rule broken");

        ex.Message.Should().Be("rule broken");
        ex.InnerException.Should().BeNull();
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldPreserveInner()
    {
        var inner = new InvalidOperationException("root");
        var ex = new SampleDomainException("wrapped", inner);

        ex.Message.Should().Be("wrapped");
        ex.InnerException.Should().BeSameAs(inner);
    }
}
