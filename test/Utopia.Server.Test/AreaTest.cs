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
using Utopia.Server.Entity;
using Utopia.Server.Logic;
using Utopia.Server.Map;

namespace Utopia.Test.Server;

public class AreaTest
{
    [Fact]
    public void AreaAccessTest()
    {
        var id = new Guuid("Test", "World");
        Area area = new(new(0,0, id));

        var x = IArea.XSize;
        var y = IArea.YSize;

        Assert.False(area.TryGetBlock(new Core.Map.Position(x, y, 0), out _));

        x--;
        y--;

        while(x != -1)
        {
            while(y != -1)
            {
                var pos = new Core.Map.Position(x, y, IArea.GroundZ);

                Assert.True(area.TryGetBlock(pos, out IBlock? block));

                Assert.Equal(pos, block!.Position.ToPos());
                y--;
            }
            x--;
            y = IArea.YSize - 1;
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(-2)]
    public void AreaUpdateTest(int z)
    {
        // prepare entity
        int ticks = 0;
        Mock<IEntity> entity = new();
        entity.Setup(entity => entity.LogicUpdate()).Callback(() => ticks++);
        var obj = entity.Object;

        // add to specific layer
        Area area = new(new());

        void Add(int z)
        {
            var layer = area.GetLayer(z);

            for (int x = 0; x != IArea.XSize; x++)
            {
                for (int y = 0; y != IArea.YSize; y++)
                {
                    var result = layer.TryGetBlock(new(x, y), out IBlock? block);

                    Assert.True(result);

                    block!.TryAddEntity(obj);
                }
            }
        }

        Add(IArea.GroundZ);
        Add(z);

        // update
        SimplyUpdater updater = new();

        area.Update(updater);

        Assert.Equal(IArea.XSize * IArea.YSize * 2, ticks);
    }
}
