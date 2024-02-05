// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;
using Utopia.Server.Entity;

namespace Utopia.Server.Map;

public class AreaLayerBuilder
{
    public Block[][] Blocks { get; init; }

    public WorldPosition Position { get; init; }

    public GenerationStage GenerationStage { get; set; }

    public AreaLayerBuilder(WorldPosition layerPosition)
    {
        Blocks = new Block[IArea.XSize][];
        Position = layerPosition;

        for (int i = 0; i != Blocks.Length; i++)
        {
            Blocks[IArea.XSize] = new Block[IArea.YSize];

            for (int j = 0; j != Blocks[i].Length; j++)
            {
                Blocks[i][j] = new Block(
                        new WorldPosition(
                            layerPosition.X + i,
                            layerPosition.Y + j,
                            layerPosition.Z,
                            layerPosition.Id
                    ));
            }
        }
    }

    public AreaLayerBuilder(IAreaLayer layer)
    {
        Position = layer.Position;

        Blocks = new Block[IArea.XSize][];

        for (int i = 0; i != Blocks.Length; i++)
        {
            Blocks[IArea.XSize] = new Block[IArea.YSize];

            for (int j = 0; j != Blocks[i].Length; j++)
            {
                _ = layer.TryGetBlock(new FlatPosition(i, j), out IBlock? block);
                Blocks[i][j] = (Block?)block ?? throw new InvalidOperationException();
            }
        }
    }

    public void ForEach(Action<IBlock, FlatPosition> action)
    {
        for (int x = 0; x != Blocks.Length; x++)
        {
            for (int y = 0; y != Blocks[x].Length; y++)
            {
                using var _ = Blocks[x][y].EnterWriteLock();
                action.Invoke(Blocks[x][y], new FlatPosition(x, y));
            }
        }
    }

    public void Fill(Func<IBlock, FlatPosition, IEntity> entityFactory) => ForEach(
                (b, p) =>
                {
                    _ = b.TryAddEntity(entityFactory.Invoke(b, p));
                });

    public Block this[int x, int y] => Blocks[x][y];

    public AreaLayer Get()
    {
        var layer = new AreaLayer(Blocks, Position);

        return layer;
    }
}
