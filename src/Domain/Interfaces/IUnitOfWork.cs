namespace Engrslan.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, IEntity<TKey>;
    IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity<int>;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}