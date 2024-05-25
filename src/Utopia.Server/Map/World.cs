// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using CommunityToolkit.Diagnostics;
using Utopia.Core;
using Utopia.Core.IO;
using Utopia.Core.Map;
using Utopia.Server.Logic;
using Zio;

namespace Utopia.Server.Map;

public class World : IWorld
{
    public required IFileSystem FileSystem { get; init; }

    private readonly Area[][] _areas;

    private readonly string _path;

    public World(Guuid id, int xSize, int ySize, IWorldGenerator generator, string path)
    {
        ArgumentNullException.ThrowIfNull(generator);
        _path = path;
        Id = id;
        XAreaCount = xSize;
        XAreaNegativeCount = xSize;
        YAreaCount = ySize;
        YAreaNegativeCount = ySize;

        _areas = new Area[xSize * 2][];

        for (int xIndex = -xSize, xAbs = 0; xIndex != xSize; xIndex++, xAbs++)
        {
            _areas[xAbs] = new Area[ySize * 2];

            for (int yIndex = -ySize, yAbs = 0; yIndex != ySize; yIndex++, yAbs++)
            {
                _areas[xAbs][yAbs] = new Area(new FlatPositionWithId(
                    xIndex * IArea.YSize,
                    yIndex * IArea.XSize,
                    id
                ));
            }
        }

        Generator = generator;
    }

    public IWorldGenerator Generator { get; init; }

    public Guuid Id { get; init; }

    public int XAreaCount { get; init; }

    public int XAreaNegativeCount { get; init; }

    public int YAreaCount { get; init; }

    public int YAreaNegativeCount { get; init; }

    public Guuid WorldType => new Guuid("Utopia", "DefaultWorld");

    private bool _InRange(FlatPosition position) => position.X < XAreaCount * IArea.XSize && position.X >= -XAreaNegativeCount * IArea.XSize
           && position.Y < YAreaCount * IArea.YSize && position.Y >= -YAreaNegativeCount * IArea.YSize;

    private static (int areaIndex, int posInArea) _GetPosInArea(int originIndex, int split)
    {
        int areaIndex;
        int posInArea;
        if (originIndex >= 0)
        {
            posInArea = originIndex % split;
            areaIndex = (int)Math.Floor((double)originIndex / split);
        }
        else
        {
            originIndex = Math.Abs(originIndex);
            areaIndex = (int)-Math.Ceiling((double)originIndex / split);
            posInArea = originIndex % split == 0 ? 0 : split - (originIndex % split);
        }
        return new(areaIndex, posInArea);
    }

    public bool TryGetArea(FlatPosition position, out IArea? area)
    {
        if (!_InRange(position))
        {
            area = null;
            return false;
        }

        int xa = _GetPosInArea(position.X, IArea.XSize).areaIndex;
        int ya = _GetPosInArea(position.Y, IArea.YSize).areaIndex;

        area = _areas[xa + XAreaCount][ya + YAreaCount];
        return true;
    }

    public bool TryGetBlock(Position position, out IBlock? block)
    {
        _ = _InRange(position.ToFlat());

        (int xArea, int xIndex) = _GetPosInArea(position.X, IArea.XSize);
        (int yArea, int yIndex) = _GetPosInArea(position.Y, IArea.YSize);

        Area area = _areas[xArea + XAreaCount][yArea + YAreaCount];
        _ = area!.TryGetBlock(new Position(xIndex, yIndex, position.Z), out block);

        // 生成世界
        IAreaLayer layer = area.GetLayer(position.Z);

        if (layer.Stage != GenerationStage.Finish)
        {
            Generator.Generate(layer);
        }

        return true;
    }

    public void Update(IUpdater updater)
    {
        Guard.IsNotNull(updater);

        foreach (Area[] x in _areas)
        {
            foreach (Area y in x)
            {
                y.Update(updater);
            }
        }
    }

    public void Save()
    {
        MemoryStream stream = new();
        foreach (Area[] x in _areas)
        {
            foreach (Area y in x)
            {
                StreamUtility.WriteInt(stream, y.Position.X).Wait();
                StreamUtility.WriteInt(stream, y.Position.Y).Wait();
                StreamUtility.WriteDataWithLength(stream, y.SaveAs()).Wait();
            }
        }
        FileSystem.WriteAllBytes(UPath.Combine(_path, "data.bin"), stream.ToArray());
    }
}
