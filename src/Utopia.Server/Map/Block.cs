// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Utopia.Core.IO;
using Utopia.Core.Map;
using Utopia.Server.Entity;

namespace Utopia.Server.Map;

public class Block(WorldPosition position) : IBlock
{
    private readonly HashSet<IEntity> _entities = new();
    private ulong _cannotAccessible = 0;
    private bool _hasCollision = false;
    private readonly ReaderWriterLockSlim _readerWriterLock = new();

    public bool HasCollision
    {
        get
        {
            return _hasCollision;
        }
    }

    public bool Accessible
    {
        get
        {
            return _cannotAccessible == 0;
        }
    }

    public WorldPosition Position { get; init; } = position;

    public ReaderWriterLockSlim @lock => _readerWriterLock;

    public bool Contains(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        return _entities.Contains(entity);
    }

    public bool Contains(Guuid idOfEntity)
    {
        return _entities.Any((e) => e.Id.Equals(idOfEntity));
    }

    public long EntityCount => _entities.Count;

    public IReadOnlyCollection<IEntity> GetAllEntities()
    {
        // to array to prevent user to change list
        return _entities.ToArray();
    }

    public void RemoveAllEntity(Guuid idOfEntity)
    {
        var removed = _entities.TakeWhile((e) => e.Id.Equals(idOfEntity));

        foreach(var item in removed)
        {
            RemoveEntity(item);
        }
    }

    public bool IsEmpty()
    {
        return _entities.Count == 0;
    }

    public void RemoveEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        bool found = _entities.Remove(entity);

        if (!found)
        {
            return;
        }
        // update
        if (entity.CanCollide)
        {
            _hasCollision = false;
        }
        if (entity.Accessible)
        {
            return;
        }

        _cannotAccessible--;
    }

    public bool TryAddEntity(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        if (_hasCollision && entity.CanCollide)
        {
            return false;
        }

        if (!_entities.Add(entity))
        {
            return false;
        }
        entity.WorldPosition = Position;

        _hasCollision = entity.CanCollide;

        if (!entity.Accessible)
        {
            _cannotAccessible++;
        }
        return true;
    }

    public void LogicUpdate()
    {
        // TODO: LOCK
        foreach (IEntity item in _entities)
        {
            item.LogicUpdate();
        }
    }

    public byte[] SaveAs()
    {
        MemoryStream stream = new();
        StreamUtility.WriteInt(stream, _entities.Count).Wait();
        foreach (IEntity item in _entities)
        {
            StreamUtility.WriteStringWithLength(stream, item.Id.ToString()).Wait();
            StreamUtility.WriteDataWithLength(stream, item.SaveAs()).Wait();
        }

        return stream.ToArray();
    }
}
