// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;

namespace Utopia.Server.Map;

/// <summary>
/// 世界由许多Area组成。Area的X层和Y层大小固定，Z层则应该是动态生成的。
/// 毕竟，谁也不想自己的电脑生成long.MAX_SIZE个数量的z层对象吧。
/// </summary>
public interface IArea : Logic.IUpdatable, ICanSave<byte[]>
{
    const int XSize = AreaInformation.XSize;
    const int YSize = AreaInformation.YSize;

    /// <summary>
    /// 地面的Z坐标。
    /// </summary>
    const int GroundZ = 0;

    bool TryGetBlock(Position position, out IBlock? block);

    IAreaLayer GetLayer(int z);

    /// <summary>
    /// Area坐标
    /// </summary>
    FlatPositionWithId Position { get; }

    IBiome Biome { get; set; }

    IPrecipitation Precipitation { get; set; }

    ITemperature Temperature { get; set; }

    IElevation Elevation { get; set; }

    IConstruction? Construction { get; set; }

}
