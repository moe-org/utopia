// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.IO;
using Utopia.Core.Map;

namespace Utopia.Server.Map;

public class Area : IArea
{
    private readonly AreaLayer _ground;
    private readonly Dictionary<int, AreaLayer> _floors = new();

    public FlatPositionWithId Position { get; init; }

    private ITemperature _temperature = new EmptyTemperature();

    public ITemperature Temperature
    {
        get
        {
            return _temperature;
        }
        set
        {
                _temperature = value;
        }
    }

    private IBiome _biome = new EmptyBiome();

    public IBiome Biome
    {
        get
        {
            return _biome;
        }
        set
        {
            _biome = value;     
        }
    }

    private IPrecipitation _precipitation = new EmptyPrecipiation();

    public IPrecipitation Precipitation
    {
        get
        {

                return _precipitation;
        }
        set
        {
                _precipitation = value;
        }
    }

    private IElevation _elevation = new EmptyElevation();

    public IElevation Elevation
    {
        get
        {
                return _elevation;
        }
        set
        {
                _elevation = value;
        }
    }

    private IConstruction? _construction = null;

    public IConstruction? Construction
    {
        get
        {
                return _construction;
        }
        set
        {
                _construction = value;
        }
    }

    public ReaderWriterLockSlim @lock { get; init; } = new();

    public Area(FlatPositionWithId worldPosition)
    {
        _ground = new(new WorldPosition(
            worldPosition.X,
            worldPosition.Y,
            IArea.GroundZ,
            worldPosition.Id));
        Position = worldPosition;
    }

    public bool TryGetBlock(Position position, out IBlock? block)
    {
        if(position.Z == IArea.GroundZ)
        {
            return _ground.TryGetBlock(position.ToFlat(), out block);
        }

        var layer = GetLayer(position.Z);

        return layer.TryGetBlock(position.ToFlat(), out block);
    }

    public IAreaLayer GetLayer(int z)
    {
        if(z == IArea.GroundZ)
        {
            return _ground;
        }

        var pos = Position;

        if (!_floors.TryGetValue(z, out AreaLayer? value))
        {
            value = new AreaLayer(
                    new WorldPosition(
                        pos.X,
                        pos.Y,
                        z,
                        pos.Id
                ));
            _floors.Add(
            z,
            value);
        }

        return value;
    }

    public void Update(Logic.IUpdater updater)
    {
        _ground.Update(updater);

        KeyValuePair<int, AreaLayer>[] floor = _floors.ToArray();

        foreach (KeyValuePair<int, AreaLayer> z in floor)
        {
            z.Value.Update(updater);
        }
    }

    public byte[] Save()
    {
        KeyValuePair<int, AreaLayer>[] floors = _floors.ToArray();
        var stream = new MemoryStream();

        foreach (KeyValuePair<int, AreaLayer> floor in floors)
        {
            StreamUtility.WriteInt(stream, floor.Key).Wait();
            StreamUtility.WriteDataWithLength(stream, floor.Value.Save()).Wait();
        }

        return stream.ToArray();
    }
}
