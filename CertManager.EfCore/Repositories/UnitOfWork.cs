using CertManager.Domain.Interfaces;
using CertManager.Domain.Shared.DependencyInjection;
using Microsoft.EntityFrameworkCore.Storage;

namespace CertManager.EfCore.Repositories;

public class UnitOfWork : IUnitOfWork, IScopedService
{
    private readonly ApplicationDataContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDataContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, IEntity<TKey>
    {
        var type = typeof(TEntity);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TEntity, TKey>(_context);
        }

        return (IRepository<TEntity, TKey>)_repositories[type];
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class, IEntity<int>
    {
        var type = typeof(TEntity);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TEntity>(_context);
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}