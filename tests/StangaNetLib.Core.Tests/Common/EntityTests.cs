using FluentAssertions;
using StangaNetLib.Core.Common;
using StangaNetLib.Core.Events;

namespace StangaNetLib.Core.Tests.Common;

file class SampleEntity : Entity
{
    public string Name { get; private set; }

    public SampleEntity(string name) { Name = name; }

    public void Rename(string newName)
    {
        Name = newName;
        MarkUpdated();
    }
}

file class SampleAggregateRoot : AggregateRoot
{
    public string Name { get; private set; }

    public SampleAggregateRoot(string name) { Name = name; }

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
    public void Equality_TwoEntitiesWithDifferentIds_ShouldNotBeEqual()
    {
        var a = new SampleEntity("A");
        var b = new SampleEntity("B");

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
    public void GetHashCode_ShouldEqualIdHashCode()
    {
        var entity = new SampleEntity("A");

        entity.GetHashCode().Should().Be(entity.Id.GetHashCode());
    }
}

public class AggregateRootTests
{
    [Fact]
    public void NewAggregateRoot_ShouldHaveNewGuid_And_CreatedAtSet()
    {
        var aggregate = new SampleAggregateRoot("Test");

        aggregate.Id.Should().NotBe(Guid.Empty);
        aggregate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        aggregate.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void AddDomainEvent_ShouldAccumulateEvents()
    {
        var aggregate = new SampleAggregateRoot("Test");
        aggregate.Rename("New");

        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Single().Should().BeOfType<SampleEvent>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var aggregate = new SampleAggregateRoot("Test");
        aggregate.Rename("New");
        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MultipleRenames_ShouldAccumulateMultipleEvents()
    {
        var aggregate = new SampleAggregateRoot("Test");
        aggregate.Rename("First");
        aggregate.Rename("Second");

        aggregate.DomainEvents.Should().HaveCount(2);
    }
}
