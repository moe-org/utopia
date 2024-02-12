// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;
using Utopia.Server.Entity;

namespace Utopia.Server.Map;

/// <summary>
/// 代表一个图块，是地图上每个坐标的所指。
/// </summary>
public interface IBlock : ICanSave<byte[]>
{
    /// <summary>
    /// Try add the entity to the block.
    /// If the entity has added to the block,ignore it and return true.
    /// If <see cref="HasCollision"/> is true and <see cref="IEntity.CanCollide"/> is true,
    /// will not add the entity and return false.
    /// </summary>
    /// <param name="entity">
    /// Its <see cref="IEntity.WorldPosition"/> will be set to <see cref="WorldPosition"/>.
    /// </param>
    bool TryAddEntity(IEntity entity);

    void RemoveEntity(IEntity entity);

    void RemoveAllEntity(Guuid idOfEntity);

    bool Contains(IEntity entity);

    bool Contains(Guuid idOfEntity);

    bool IsEmpty();

    int EntityCount { get; }

    IReadOnlyCollection<IEntity> GetAllEntities();

    bool HasCollision { get; }

    bool Accessible { get; }

    void LogicUpdate();

    WorldPosition Position { get; }
}
