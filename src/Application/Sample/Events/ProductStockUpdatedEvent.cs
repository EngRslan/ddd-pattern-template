using Engrslan.Events;

namespace Engrslan.Sample.Events;

public class ProductStockUpdatedEvent(Guid ProductId, int PreviousQuantity, int NewQuantity) : DomainEventBase;