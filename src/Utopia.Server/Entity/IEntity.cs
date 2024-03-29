// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;

namespace Utopia.Server.Entity;

/// <summary>
/// 实体信息接口。
/// </summary>
public interface IEntityInformation
{
    /// <summary>
    /// 实体名称，用于显示给玩家。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The description of the entity
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 对于每一种实体，都需要一种Id与其对应，作为唯一标识符。
    /// </summary>
    Guuid Id { get; }
}

/// <summary>
/// 物理上的实体
/// </summary>
public interface IPhysicsEntity
{
    /// <summary>
    /// 实体是否可供生物等其他实体通过。
    /// </summary>
    bool Accessible { get; }

    /// <summary>
    /// 实体是否可和其他可碰撞的实体进行碰撞。
    /// </summary>
    bool CanCollide { get; }
}

/// <summary>
/// 游戏实体接口。
/// 游戏实体是任何出现在地图上的可互动的“东西”。
/// 一些视觉特效等不算实体，但是前提是这些“特效”在有和没有的情况下对游戏逻辑不造成可观测的影响。
/// 典型实体如：生物，玩家，掉落物，建筑。
/// </summary>
public interface IEntity : ICanSave<byte[]>, IEntityInformation, IPhysicsEntity, IEquatable<IEntity>
{
    /// <summary>
    /// 每个逻辑帧调用。一秒20个逻辑帧，可能从不同线程发起调用。
    /// </summary>
    void LogicUpdate();

    /// <summary>
    /// 世界坐标。
    /// </summary>
    WorldPosition WorldPosition { get; set; }
}
