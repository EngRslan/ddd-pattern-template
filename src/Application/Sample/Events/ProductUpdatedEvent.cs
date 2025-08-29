using Engrslan.Events;

namespace Engrslan.Sample.Events;

public class ProductUpdatedEvent(Guid ProductId, string Name) : DomainEventBase;