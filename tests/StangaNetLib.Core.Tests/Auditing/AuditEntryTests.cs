using FluentAssertions;
using StangaNetLib.Core.Auditing;

namespace StangaNetLib.Core.Tests.Auditing;

public class AuditEntryTests
{
    [Fact]
    public void Defaults_ShouldInitializeIdAndOccurredAt()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var entry = new AuditEntry();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        entry.Id.Should().NotBe(Guid.Empty);
        entry.OccurredAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entry.UserId.Should().BeEmpty();
        entry.Action.Should().BeEmpty();
        entry.EntityType.Should().BeNull();
        entry.EntityId.Should().BeNull();
        entry.Details.Should().BeNull();
        entry.IpAddress.Should().BeNull();
        entry.UserAgent.Should().BeNull();
    }

    [Fact]
    public void InitProperties_ShouldBeSettableViaObjectInitializer()
    {
        var occurred = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var entry = new AuditEntry
        {
            Id = id,
            UserId = "user-42",
            Action = "consent.grant",
            EntityType = "Consent",
            EntityId = "c-9",
            Details = "{\"scope\":\"marketing\"}",
            IpAddress = "203.0.113.10",
            UserAgent = "TestAgent/1.0",
            OccurredAt = occurred
        };

        entry.Id.Should().Be(id);
        entry.UserId.Should().Be("user-42");
        entry.Action.Should().Be("consent.grant");
        entry.EntityType.Should().Be("Consent");
        entry.EntityId.Should().Be("c-9");
        entry.Details.Should().Be("{\"scope\":\"marketing\"}");
        entry.IpAddress.Should().Be("203.0.113.10");
        entry.UserAgent.Should().Be("TestAgent/1.0");
        entry.OccurredAt.Should().Be(occurred);
    }

    [Fact]
    public void RecordEquality_ShouldCompareByValue()
    {
        var id = Guid.NewGuid();
        var occurred = DateTimeOffset.UtcNow;

        var left = new AuditEntry
        {
            Id = id,
            UserId = "u1",
            Action = "data.export",
            OccurredAt = occurred
        };
        var right = new AuditEntry
        {
            Id = id,
            UserId = "u1",
            Action = "data.export",
            OccurredAt = occurred
        };

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }
}
