using System.Linq.Expressions;
using Api.Contracts.Common;
using Application.Common.Pagination;
using Domain.Common;

namespace Application.Abstractions.Repositories;

public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object?>>[] includes);

    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object?>>[] includes);

    Task<TResponse?> ProjectToSingleOrDefaultAsync<TResponse>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TResponse : class;

    Task<PagedResult<TResponse>> ProjectToPagedAsync<TResponse>(
        PaginationRequest pagination,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool descending = false,
        CancellationToken cancellationToken = default)
        where TResponse : class;

    Task<IReadOnlyList<TResponse>> ProjectToListAsync<TResponse>(
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool descending = false,
        CancellationToken cancellationToken = default)
        where TResponse : class;

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    void Remove(TEntity entity);
}
