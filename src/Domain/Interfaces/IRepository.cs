using System.Linq.Expressions;

namespace Engrslan.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
    Task<PagedResult<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, Expression<Func<TEntity, object>>? orderBy = null, bool ascending = true, CancellationToken cancellationToken = default);
    Task<TEntity> InsertAsync(TEntity entity, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task DeleteAsync(TKey id, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, bool saveImmediately = true, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Query();
    IQueryable<TEntity> QueryNoTracking();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity> : IRepository<TEntity, int> where TEntity : class, IEntity<int>
{
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}