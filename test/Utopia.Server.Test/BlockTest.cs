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

/// <summary>
/// Test for <see cref="Utopia.Server.Map.Block"/>
/// </summary>
public class BlockTest
{
    // for tests
    private static Guuid DefaultID = new Guuid("Test","Default");

    private static Guuid GuuidOne = new Guuid("Test", "One");

    private static Guuid GuuidTwo = new Guuid("Test", "Two");

    public Block NewBlock(WorldPosition? worldPosition = null)
    {
        return new Block(worldPosition ?? new(0, 0, 0, DefaultID));
    }

    private Mock<IEntity> DefaultEntity(
        bool accessible,
        bool canCollide,
        Guuid? id = null,
        WorldPosition? worldPosition = null)
    {
        Mock<IEntity> entity = new();
        entity.Setup(entity => entity.Accessible).Returns(accessible);
        entity.Setup(entity => entity.CanCollide).Returns(canCollide);

        if (worldPosition != null)
        {
            entity.SetupProperty(entity => entity.WorldPosition, worldPosition.Value);
        }

        if (id != null)
        {
            entity.Setup(entity => entity.Id).Returns(id.Value);
        }
        return entity;
    }

    [Fact]
    public void AddEntityTest()
    {
        // give a different initial value
        var one = new Guuid("Test","One");
        var two = new Guuid("Test", "Two");
        Block block = NewBlock(new(0,0,0, one));
        var obj = DefaultEntity(false, false,null,new(1,1,1, two)).Object;
        var pos = block.Position;

        Assert.NotEqual(pos, obj.WorldPosition);
        Assert.True(block.TryAddEntity(obj));
        Assert.Equal(pos,obj.WorldPosition);
    }

    [Fact]
    public void AddSameEntityToBlockTest()
    {
        Block block = NewBlock();
        var obj = DefaultEntity(false,false).Object;

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

        var obj1 = DefaultEntity(true,true).Object;

        var obj2 = DefaultEntity(true,true).Object;

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

        var obj1 = DefaultEntity(true,true).Object;

        Assert.True(block.Accessible);
        Assert.False(block.HasCollision);

        Assert.True(block.TryAddEntity(obj1));
        Assert.True(block.Accessible);
        Assert.True(block.HasCollision);

        block.RemoveEntity(obj1);
        Assert.True(block.Accessible);
        Assert.False(block.HasCollision);
    }

    [Fact]
    public void ContainEntityTest()
    {
        Block block = NewBlock();

        var entity = DefaultEntity(false, false,GuuidOne);
        var obj = entity.Object;

        var entity2 = DefaultEntity(false, false, GuuidTwo);
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

        var one = DefaultEntity(false, false, GuuidOne).Object;
        var two = DefaultEntity(false, false, GuuidOne).Object;
        var another = DefaultEntity(false, false, GuuidTwo).Object;

        Assert.True(block.TryAddEntity(one));
        Assert.True(block.TryAddEntity(two));
        Assert.True(block.TryAddEntity(another));
        Assert.Equal(3,block.EntityCount);

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
