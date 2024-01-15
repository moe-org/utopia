// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Exceptions;
public class EntityNotFoundException : System.Exception
{
    public Guuid EntityID { get; init; }

    public EntityNotFoundException(Guuid entityId) : base($"the entity {entityId} not found") => EntityID = entityId;
}
