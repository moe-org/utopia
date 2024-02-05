// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;

namespace Utopia.Server.Map;

/// <summary>
/// 世界接口
/// </summary>
public interface IWorld : Logic.IUpdatable, ICanSave
{

    /// <summary>
    /// 世界类型
    /// </summary>
    Guuid WorldType { get; }

    /// <summary>
    /// 世界ID
    /// </summary>
    Guuid Id { get; }

    /// <summary>
    /// X轴世界大小，单位为区域
    /// </summary>
    int XAreaCount { get; }

    /// <summary>
    /// X轴世界负半轴大小，单位为区域
    /// </summary>
    int XAreaNegativeCount { get; }

    /// <summary>
    /// Y轴世界大小，单位为区域
    /// </summary>
    int YAreaCount { get; }

    /// <summary>
    /// Y轴世界负半轴大小，单位为区域
    /// </summary>
    int YAreaNegativeCount { get; }

    bool TryGetArea(FlatPosition position, out IArea? area);

    bool TryGetBlock(Position position, out IBlock? block);

    /// <summary>
    /// 世界生成器
    /// </summary>
    IWorldGenerator Generator { get; }
}
