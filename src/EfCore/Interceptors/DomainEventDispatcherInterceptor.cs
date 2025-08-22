using Engrslan.Domain.Interfaces;
using Engrslan.Domain.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Engrslan.EfCore.Interceptors;

public class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IEventDispatcher _eventDispatcher;

    public DomainEventDispatcherInterceptor(IEventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // We'll dispatch events after successful save
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext != null)
        {
            await DispatchDomainEventsAsync(dbContext, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var aggregateRoots = context.ChangeTracker
            .Entries<IAggregateRootEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.GetAndClearDomainEvents())
            .ToList();

        if (domainEvents.Count != 0)
        {
            await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
    }
}