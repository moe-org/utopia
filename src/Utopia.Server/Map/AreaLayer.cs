// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.IO;
using Utopia.Core.Map;
using Utopia.Server.Logic;
using static System.Reflection.Metadata.BlobBuilder;

namespace Utopia.Server.Map;

[System.Runtime.CompilerServices.InlineArray(IArea.XSize)]
internal struct AreaLayerBlockLine
{
    public Block element;
}

[System.Runtime.CompilerServices.InlineArray(IArea.YSize)]
internal struct AreaLayerBlocks
{
    public AreaLayerBlockLine element;
}

public class AreaLayer : IAreaLayer
{
    private readonly object _lock = new();
    private readonly AreaLayerBlocks _blocks;

    public WorldPosition Position { get; init; }

    private GenerationStage _stage = GenerationStage.NoStart;

    public GenerationStage Stage
    {
        get
        {
            lock (_lock)
            {
                return _stage;
            }
        }
        set
        {
            lock (_lock)
            {
                _stage = value;
            }
        }
    }

    internal AreaLayer(Block[][] blocks, WorldPosition position)
    {
        int yIndex = 0;
        foreach(var y in blocks)
        {
            int xIndex = 0;
            foreach(var x in y)
            {
                _blocks[yIndex][xIndex] = x;
                xIndex++;
            }
            yIndex++;
        }

        Position = position;
    }

    public AreaLayer(WorldPosition position)
    {
        Position = position;

        for(int line = 0;line != IArea.YSize;line++)
        {
            for(int x = 0;x != IArea.XSize; x++)
            {
                _blocks[line][x] = new(new(position.X + x,position.Y + line,position.Z,position.Id));
            }
        }
    }

    public bool TryGetBlock(FlatPosition position, out IBlock? block)
    {
        if (position.X < 0 || position.X >= IArea.XSize
            || position.Y < 0 || position.Y >= IArea.YSize)
        {
            block = null;
            return false;
        }

        block = _blocks[position.Y][position.X];

        return true;
    }

    public void Update(IUpdater updater)
    {
        foreach (var blocks in _blocks)
        {
            foreach (IBlock block in blocks)
            {
                updater.AssignTask(block.LogicUpdate);
            }
        }
    }

    public byte[] Save()
    {
        MemoryStream stream = new();

        foreach (AreaLayerBlockLine x in _blocks)
        {
            foreach (Block y in x)
            {
                StreamUtility.WriteDataWithLength(stream, y.Save()).Wait();
            }
        }

        return stream.ToArray();
    }
}
