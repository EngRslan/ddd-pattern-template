using CertManager.Domain.Interfaces;
using CertManager.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CertManager.EfCore.Interceptors;

public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;

    public AuditableEntitySaveChangesInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var currentUser = _serviceProvider.GetService<ICurrentUser>();
        var dateTimeService = _serviceProvider.GetRequiredService<IDateTimeService>();
        var currentUserId = ParseUserIdAsGuid(currentUser?.Id);
        var currentTime = dateTimeService.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                HandleAddedEntity(entry, currentUserId, currentTime);
            }
            else if (entry.State == EntityState.Modified)
            {
                HandleModifiedEntity(entry, currentUserId, currentTime);
            }
            else if (entry.State == EntityState.Deleted)
            {
                HandleDeletedEntity(entry, currentUserId, currentTime);
            }

            HandleConcurrencyStamp(entry);
        }
    }

    private static void HandleAddedEntity(EntityEntry entry, Guid? userId, DateTime currentTime)
    {
        if (entry.Entity is ICreationEntity<int> creationEntity)
        {
            creationEntity.CreatedBy = userId;
            creationEntity.CreatedAt = currentTime;
        }

        if (entry.Entity is IAuditableEntity<int> auditableEntity)
        {
            auditableEntity.UpdatedBy = null;
            auditableEntity.UpdatedAt = null;
        }

        if (entry.Entity is IFullAuditableEntity<int> fullAuditableEntity)
        {
            fullAuditableEntity.IsDeleted = false;
            fullAuditableEntity.DeletedBy = null;
            fullAuditableEntity.DeletedAt = null;
        }
    }

    private static void HandleModifiedEntity(EntityEntry entry, Guid? userId, DateTime currentTime)
    {
        if (entry.Entity is IAuditableEntity<int> auditableEntity)
        {
            auditableEntity.UpdatedBy = userId;
            auditableEntity.UpdatedAt = currentTime;
            
            if (entry.Entity is ICreationEntity<int>)
            {
                entry.Property(nameof(ICreationEntity<int>.CreatedBy)).IsModified = false;
                entry.Property(nameof(ICreationEntity<int>.CreatedAt)).IsModified = false;
            }
        }
    }

    private static void HandleDeletedEntity(EntityEntry entry, Guid? userId, DateTime currentTime)
    {
        if (entry.Entity is IFullAuditableEntity<int> fullAuditableEntity)
        {
            entry.State = EntityState.Modified;
            fullAuditableEntity.IsDeleted = true;
            fullAuditableEntity.DeletedBy = userId;
            fullAuditableEntity.DeletedAt = currentTime;
            fullAuditableEntity.UpdatedBy = userId;
            fullAuditableEntity.UpdatedAt = currentTime;
        }
    }

    private static void HandleConcurrencyStamp(EntityEntry entry)
    {
        if (entry.Entity is IHasConcurrencyStamp concurrencyEntity && 
            (entry.State == EntityState.Added || entry.State == EntityState.Modified))
        {
            concurrencyEntity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }
    }

    private static Guid? ParseUserIdAsGuid(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return null;

        return Guid.TryParse(userId, out var guid) ? guid : null;
    }
}