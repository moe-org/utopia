// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;

namespace Utopia.Server.Map;

/// <summary>
/// 代表一个Z轴下的Area
/// </summary>
public interface IAreaLayer : Logic.IUpdatable, ICanSave<byte[]>
{
    bool TryGetBlock(FlatPosition position, out IBlock? block);

    /// <summary>
    /// Area坐标
    /// </summary>
    WorldPosition Position { get; }

    /// <summary>
    /// 标识目前区域所处的阶段
    /// </summary>
    GenerationStage Stage { get; set; }
}
