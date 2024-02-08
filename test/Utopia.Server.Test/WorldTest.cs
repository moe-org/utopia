// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Utopia.Core.Map;
using Utopia.Server.Entity;
using Utopia.Server.Map;

namespace Utopia.Test.Server;
public class WorldTest
{
    [Fact]
    public void WorldAccessTest()
    {
        var generator = new Mock<IWorldGenerator>();
        generator.Setup(generator => generator.Generate(It.IsAny<IAreaLayer>())).Callback<IAreaLayer>(
            (layer) => layer.Stage = GenerationStage.Finish);

        World world = new(new Core.Guuid("Test","Default"), 2, 2, generator.Object, ".");

        var yIndex = -world.YAreaNegativeCount * IArea.YSize;

        while(yIndex != ((world.YAreaCount * IArea.YSize) - 1))
        {
            var xIndex = -world.XAreaNegativeCount * IArea.XSize;
            while (xIndex != ((world.XAreaCount * IArea.XSize) - 1))
            {
                // ensure the pos are the same
                var pos = new Position(xIndex, yIndex, IArea.GroundZ);
                Assert.True(world.TryGetBlock(pos, out IBlock? block));
                Assert.True(world.TryGetArea(pos.ToFlat(), out IArea? area));

                Assert.Equal(GenerationStage.Finish,area!.GetLayer(IArea.GroundZ).Stage);
                Assert.Equal(pos, block!.Position.ToPos());

                xIndex++;
            }
            yIndex++;
        }
    }
}
