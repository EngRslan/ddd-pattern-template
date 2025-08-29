using Engrslan.Events;

namespace Engrslan.Sample.Events;

public class ProductDeletedEvent(Guid ProductId) : DomainEventBase;
