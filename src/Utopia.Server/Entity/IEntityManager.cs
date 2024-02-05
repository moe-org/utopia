// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Utopia.Core;
using Utopia.Server.Map;

namespace Utopia.Server.Entity;

/// <summary>
/// 实体管理器
/// </summary>
public interface IEntityManager
{
    /// <summary>
    /// All registered entities
    /// </summary>
    IReadOnlyDictionary<Guuid, (IEntityInformation,IEntityFactory)> Entities { get; }

    /// <returns>true if add successfully,otherwise false. If a entity was registered before,it return false</returns>
    bool Register(Guuid entityId, IEntityInformation information, IEntityFactory factory);

    bool TryGet(Guuid entityId, out (IEntityInformation, IEntityFactory) get);
}

public class EntityManager : IEntityManager
{
    private ConcurrentDictionary<Guuid, (IEntityInformation,IEntityFactory)> _all = new();

    public IReadOnlyDictionary<Guuid, (IEntityInformation, IEntityFactory)> Entities => _all;

    public bool Register(Guuid entityId, IEntityInformation information, IEntityFactory factory)
    {
        ArgumentNullException.ThrowIfNull(information);
        ArgumentNullException.ThrowIfNull(factory);
        return _all.TryAdd(entityId, (information, factory));
    }

    public bool TryGet(Guuid entityId, out (IEntityInformation, IEntityFactory) get)
    {
        return Entities.TryGetValue(entityId, out get);
    }
}

