// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.Map;
using Utopia.Core.Translation;

namespace Utopia.Server.Map;

/// <summary>
/// 标识生成世界的阶段
/// </summary>
public enum GenerationStage
{
    NoStart,
    Before,
    Climate,
    Biome,
    Construction,
    Feature,
    Bio,
    Post,
    Finish,
}

/// <summary>
/// 气候
/// </summary>
public interface IPrecipitation : IPrecipitationInfo
{
}

/// <summary>
/// 海拔
/// </summary>
public interface IElevation : IElevationInfo
{
}

public interface ITemperature : ITemperatureInfo
{
}

/// <summary>
/// 生态群落
/// </summary>
public interface IBiome : IBiomeInfo
{
}

public interface IConstruction : IConstructionInfo
{
}

public class EmptyWorldInfo
{
#pragma warning disable CA1822 // Mark members as static
    public string Name => new string("Empty World Info for DEBUG");

    public Guuid ID => new Guuid("Utopia","World", "Empty");
#pragma warning restore CA1822 // Mark members as static
}

public class EmptyPrecipiation : EmptyWorldInfo, IPrecipitation
{
}
public class EmptyElevation : EmptyWorldInfo, IElevation
{
}
public class EmptyBiome : EmptyWorldInfo, IBiome
{
}
public class EmptyTemperature : EmptyWorldInfo, ITemperature
{
}
public class EmptyConstruction : EmptyWorldInfo, IConstruction
{
}

/// <summary>
/// 生态系统生成器生成器
/// </summary>
public interface IClimateGenerator
{
    /// <summary>
    /// 获取降水量
    /// </summary>
    /// <param name="position">要获取高度的位置.单位为Area</param>
    IPrecipitation GetPrecipitation(Position position);

    /// <summary>
    /// 获取海拔高度
    /// </summary>
    /// <param name="position">要获取高度的位置.单位为Area</param>
    IElevation GetElevation(Position position);

    /// <summary>
    /// 获取气温
    /// </summary>
    /// <param name="position">要获取气温的位置.单位为Area</param>
    ITemperature GetTemperature(Position position);
}

/// <summary>
/// 生态群落生成器.主要用于生成自然环境
/// </summary>
public interface IBiomeGenerator
{
    /// <summary>
    /// 获取生态系统
    /// </summary>
    /// <param name="position">单位为Area</param>
    IBiome GetBiome(FlatPositionWithId position);
}

/// <summary>
/// 结构生成器.
/// 这些结构会在地图上被表明,有自己的名字.
/// </summary>
public interface IConstructionGenerator
{
    /// <summary>
    /// 获取建筑类型.
    /// </summary>
    /// <param name="position">单位为Area</param>
    IConstruction GetConstruction(FlatPositionWithId position);
}

/// <summary>
/// 地形特征.(一般是小型的东西,且不分人造和自然物,例如树木,水井之类的).
/// 这些东西不会在地图上被标注,也没有名字.
/// </summary>
public interface IBiomeFeatureGenerator
{
    void Generate(IAreaLayer areaLayer);
}

/// <summary>
/// 世界生成器
/// </summary>
public interface IWorldGenerator
{
    /// <summary>
    /// 生成区域
    /// </summary>
    /// <param name="area">要生成的区域的位置.单位为Area.</param>
    void Generate(IAreaLayer area);
}
