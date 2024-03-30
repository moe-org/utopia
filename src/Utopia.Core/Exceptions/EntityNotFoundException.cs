namespace Utopia.Core.Exceptions;

public class EntityNotFoundException(Guuid entityId) : Exception($"the entity {entityId} not found")
{
    public Guuid EntityId { get; init; } = entityId;
}
