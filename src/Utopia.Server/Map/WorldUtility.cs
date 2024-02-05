// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using Utopia.Core;
using Utopia.Core.Map;

namespace Utopia.Server.Map;
public static class WorldUtility
{

    /// <summary>
    /// 获取坐标，自动查询世界
    /// </summary>
    /// <param name=""></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static bool TryGetBlock(this ConcurrentDictionary<Guuid,IWorld> worlds, WorldPosition position, out IBlock? block)
    {
        block = null;
        return worlds.TryGetValue(position.Id, out IWorld? w) && w!.TryGetBlock(position.ToPos(), out block);
    }

}
