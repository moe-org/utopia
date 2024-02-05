// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.Map;
using Utopia.Server.Entity;

namespace Utopia.Server.Map;

public class ClimateGenerator : IClimateGenerator
{
    public IElevation GetElevation(Position position) => new EmptyElevation();

    public IPrecipitation GetPrecipitation(Position position) => new EmptyPrecipiation();

    public ITemperature GetTemperature(Position position) => new EmptyTemperature();
}

public class Generator : IWorldGenerator
{

    private readonly IEntityManager _entityManager;

    public Generator(IEntityManager entityManager)
    {
        ArgumentNullException.ThrowIfNull(entityManager);
        _entityManager = entityManager;
    }

    public void Generate(IAreaLayer area)
    {
        if (area.Stage == GenerationStage.Finish)
        {
            return;
        }

        AreaLayerBuilder areaLayer = new(area);

        if (area.Position.Z == IArea.GroundZ)
        {
            areaLayer.Fill(
                (b, i) =>
                {
                    _entityManager.TryGet(ResourcePack.Entity.GrassEntity.ID, out var entity);
                    return entity.Item2.Create(ResourcePack.Entity.GrassEntity.ID, null);
                }
            );
        }
        else if (area.Position.Z > IArea.GroundZ)
        {
            // fill `AIR`
            return;
        }
        else
        {
            // TODO:FILL STONE
            areaLayer.Fill(
                (b, i) =>
                {
                    _entityManager.TryGet(ResourcePack.Entity.GrassEntity.ID, out var entity);
                    return entity.Item2.Create(ResourcePack.Entity.GrassEntity.ID, null);
                }
            );
        }
    }
}
