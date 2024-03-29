// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Utopia.Core;
using Utopia.Core.Map;
using Utopia.Server.Entity;
using Utopia.Server.Map;

namespace Utopia.Test.Server;

internal class TestEntity(WorldPosition position, int hashCode, bool accessible, bool canCollide, Guuid id, Action update) : IEntity
{
    public WorldPosition WorldPosition { get; set; } = position;

    public string Name => "Test Entity";

    public string Description => "A entity for test";

    public Guuid Id => id;

    public bool Accessible => accessible;

    public bool CanCollide => canCollide;

    public bool Equals(IEntity? other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetHashCode() != GetHashCode())
        {
            return false;
        }

        return true;
    }

    public void LogicUpdate() => update.Invoke();

    public override int GetHashCode() => hashCode;

    public byte[] SaveAs() => throw new NotImplementedException();
}


/// <summary>
/// Test for <see cref="Utopia.Server.Map.Block"/>
/// </summary>
public class BlockTest
{
    // for tests
    private static int HashCode = 0;

    private static Guuid DefaultID = new Guuid("Test", "Default");

    private static Guuid GuuidOne = new Guuid("Test", "One");

    private static Guuid GuuidTwo = new Guuid("Test", "Two");

    public Block NewBlock(WorldPosition? worldPosition = null)
    {
        return new Block(worldPosition ?? new(0, 0, 0, DefaultID));
    }

    private Mock<IEntity> NewEntity(
        bool accessible,
        bool canCollide,
        Guuid? id = null,
        WorldPosition? worldPosition = null)
    {
        int hashcode = Interlocked.Increment(ref HashCode);
        id = id != null ? id.Value : DefaultID;

        Mock<IEntity> entity = new();
        entity.Setup(entity => entity.Accessible).Returns(accessible);
        entity.Setup(entity => entity.CanCollide).Returns(canCollide);
        entity.Setup(entity => entity.GetHashCode()).Returns(hashcode);
        entity.Setup(entity => entity.Equals(It.IsAny<IEntity?>())).Returns((IEntity? other)
            => other != null
            && other.GetType().IsAssignableTo(typeof(IEntity))
            && other.Id.Equals(id)
            && other.GetHashCode() == hashcode);
        entity.Setup(entity => entity.Equals(It.IsAny<object?>())).Returns((object? other)
            => other != null
            && other.GetType().IsAssignableTo(typeof(IEntity))
            && ((IEntity)other).Id.Equals(other)
            && other.GetHashCode() == hashcode);

        if (worldPosition != null)
        {
            entity.SetupProperty(entity => entity.WorldPosition, worldPosition.Value);
        }

        entity.Setup(entity => entity.Id).Returns(id.Value);

        return entity;
    }

    [Fact]
    public void AddEntityTest()
    {
        // give a different initial value
        var one = new Guuid("Test", "One");
        var two = new Guuid("Test", "Two");
        Block block = NewBlock(new(0, 0, 0, one));
        var obj = NewEntity(false, false, null, new(1, 1, 1, two)).Object;
        var pos = block.Position;

        Assert.NotEqual(pos, obj.WorldPosition);
        Assert.True(block.TryAddEntity(obj));
        Assert.Equal(pos, obj.WorldPosition);
    }

    [Fact]
    public void AddSameEntityToBlockTest()
    {
        Block block = NewBlock();
        var origin = NewEntity(false, false, GuuidOne, new WorldPosition(0, 1, 2, GuuidTwo));
        var obj = origin.Object;

        Assert.Equal(obj, obj);
        Assert.Equal(obj.GetHashCode(), obj.GetHashCode());
        Assert.Equal(obj.WorldPosition, obj.WorldPosition);
        Assert.Equal(obj.Id, obj.Id);
        Assert.True(obj.Equals(obj));

        Assert.Equal(0, block.EntityCount);

        Assert.True(block.TryAddEntity(obj));
        Assert.Equal(1, block.EntityCount);

        Assert.False(block.TryAddEntity(obj));
        Assert.Equal(1, block.EntityCount);
    }

    [Fact]
    public void AddTwoCollisionsToBlockTest()
    {
        Block block = NewBlock();

        var obj1 = NewEntity(true, true).Object;

        var obj2 = NewEntity(true, true).Object;

        Assert.Equal(0, block.EntityCount);

        Assert.True(block.TryAddEntity(obj1));
        Assert.Equal(1, block.EntityCount);
        Assert.True(block.HasCollision);

        Assert.False(block.TryAddEntity(obj2));
        Assert.Equal(1, block.EntityCount);
    }

    [Fact]
    public void CollisionCheckTest()
    {
        Block block = NewBlock();

        var obj = NewEntity(true, true).Object;

        Assert.True(block.Accessible);
        Assert.False(block.HasCollision);

        Assert.True(block.TryAddEntity(obj));
        Assert.Equal(1, block.EntityCount);
        Assert.True(block.Accessible);
        Assert.True(block.HasCollision);

        Assert.True(block.RemoveEntity(obj));
        Assert.True(block.IsEmpty());
        Assert.Equal(0, block.EntityCount);
        Assert.True(block.Accessible);
        Assert.False(block.HasCollision);
    }

    [Fact]
    public void ContainEntityTest()
    {
        Block block = NewBlock();

        var entity = NewEntity(false, false, GuuidOne);
        var obj = entity.Object;

        var entity2 = NewEntity(false, false, GuuidTwo);
        var obj2 = entity2.Object;

        Assert.True(block.TryAddEntity(obj));
        Assert.True(block.Contains(obj));
        Assert.False(block.Contains(obj2));
        Assert.True(block.Contains(GuuidOne));
        Assert.False(block.Contains(GuuidTwo));

        Assert.True(block.TryAddEntity(obj2));
        Assert.True(block.Contains(obj2));
        Assert.True(block.Contains(GuuidTwo));
    }

    [Fact]
    public void RemoveEntityTest()
    {
        Block block = NewBlock();

        var one = NewEntity(false, false, GuuidOne).Object;
        var two = NewEntity(false, false, GuuidOne).Object;
        var another = NewEntity(false, false, GuuidTwo).Object;

        Assert.True(block.TryAddEntity(one));
        Assert.True(block.TryAddEntity(two));
        Assert.True(block.TryAddEntity(another));
        Assert.Equal(3, block.EntityCount);

        block.RemoveAllEntity(GuuidOne);

        Assert.False(block.Contains(one));
        Assert.False(block.Contains(two));
        Assert.True(block.Contains(another));
        Assert.Equal(1, block.EntityCount);

        block.RemoveEntity(another);

        Assert.True(block.IsEmpty());
        Assert.Equal(0, block.EntityCount);
    }
}
