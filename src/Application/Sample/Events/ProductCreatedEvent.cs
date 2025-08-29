using Engrslan.Events;

namespace Engrslan.Sample.Events;

public class ProductCreatedEvent(Guid ProductId, string Name, string Sku) : DomainEventBase;