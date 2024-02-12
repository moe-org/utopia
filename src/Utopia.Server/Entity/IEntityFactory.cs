// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using Utopia.Core;
using Utopia.Core.Exceptions;

namespace Utopia.Server.Entity;

/// <summary>
/// 实体工厂接口
/// </summary>
public interface IEntityFactory
{
    /// <summary>
    /// 生产一个实体
    /// </summary>
    /// <param name="entityId">想要创建的实体的ID</param>
    /// <param name="data">实体数据，通常是从存档中加载的。
    /// </param>
    /// <returns>实体</returns>
    IEntity Create(Guuid entityId, byte[]? data);
}

public class EmptyEntityFactory : IEntityFactory
{
    public ConcurrentDictionary<Guuid, IEntity> Entities { get; }
        = new();

    public IEntity Create(Guuid entityId, byte[]? data)
    {
        return Entities.TryGetValue(entityId, out IEntity? entity) ? entity! : throw new EntityNotFoundException(entityId);
    }
}
