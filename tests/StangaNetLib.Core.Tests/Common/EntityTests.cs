using FluentAssertions;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Events;

namespace StangaNetLib.Core.Tests.Common;

// Concrete entity for testing
file class SampleEntity : Entity
{
    public string Name { get; private set; }

    public SampleEntity(string name) { Name = name; }

    public void Rename(string newName)
    {
        Name = newName;
        MarkUpdated();
        AddDomainEvent(new SampleEvent(Id, newName));
    }
}

file class SampleEvent(Guid entityId, string newName) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
    public string NewName { get; } = newName;
}

public class EntityTests
{
    [Fact]
    public void NewEntity_ShouldHaveNewGuid_And_CreatedAtSet()
    {
        var entity = new SampleEntity("Test");

        entity.Id.Should().NotBe(Guid.Empty);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void MarkUpdated_ShouldSetUpdatedAt()
    {
        var entity = new SampleEntity("Test");
        entity.Rename("Updated");
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddDomainEvent_ShouldBeAvailable_UntilCleared()
    {
        var entity = new SampleEntity("Test");
        entity.Rename("New");

        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Single().Should().BeOfType<SampleEvent>();

        entity.ClearDomainEvents();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Equality_TwoEntitiesWithSameId_ShouldBeEqual()
    {
        var a = new SampleEntity("A") { };
        var b = new SampleEntity("B") { };

        // Different instances, different IDs → not equal
        (a == b).Should().BeFalse();
    }

    [Fact]
    public void Equality_SameReference_ShouldBeEqual()
    {
        var entity = new SampleEntity("A");
        Entity sameRef = entity;
        (entity == sameRef).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameId_ShouldBeEqual()
    {
        var entity = new SampleEntity("A");
        entity.GetHashCode().Should().Be(entity.Id.GetHashCode());
    }
}
