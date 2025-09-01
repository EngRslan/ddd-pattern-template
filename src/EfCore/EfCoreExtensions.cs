using Engrslan.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engrslan;

public static class EfCoreExtensions
{
    public static void Config<TEntity>(this EntityTypeBuilder<TEntity> builder,string? tableName = null,string? schemaName = null) 
        where TEntity : class
    {

        if (typeof(IEntity<>).IsAssignableFrom(builder.Metadata.ClrType))
        {
            builder.HasKey(nameof(IEntity.Id));
        }

        if (typeof(ICreationAuditedEntity).IsAssignableFrom(builder.Metadata.ClrType))
        {
            builder.Property(nameof(ICreationAuditedEntity.CreatedAt)).IsRequired().HasDefaultValueSql("GETDATE()");
            builder.Property(nameof(ICreationAuditedEntity.CreatedBy)).IsRequired(false);
        }

        if (typeof(IAuditedEntity).IsAssignableFrom(builder.Metadata.ClrType))
        {
            builder.Property(nameof(IAuditedEntity.ModifiedAt)).IsRequired(false);
            builder.Property(nameof(IAuditedEntity.ModifiedBy)).IsRequired(false);
        }
        
        if (typeof(IFullAuditedEntity).IsAssignableFrom(builder.Metadata.ClrType))
        {
            builder.Property(nameof(IFullAuditedEntity.IsDeleted)).HasDefaultValue(false);
            builder.Property(nameof(IFullAuditedEntity.DeletedAt)).IsRequired(false);
            builder.Property(nameof(IFullAuditedEntity.DeletedBy)).IsRequired(false);
            builder.HasQueryFilter(e => EF.Property<bool>(e, nameof(IFullAuditedEntity.IsDeleted)) == false);
        }

        if (typeof(IAggregateRoot).IsAssignableFrom(builder.Metadata.ClrType))
        {
            builder.Ignore(nameof(IAggregateRoot.DomainEvents));
        }

        if (!string.IsNullOrEmpty(tableName))
        {
            builder.ToTable(tableName,schemaName);
        }
    }
}