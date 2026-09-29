using System.Linq.Expressions;
using Api.Contracts.Common;
using Application.Abstractions.Repositories;
using Application.Common.Pagination;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbSet<TEntity> _dbSet;
    private readonly IConfigurationProvider _mapperConfiguration;

    public Repository(
        ApplicationDbContext context,
        IConfigurationProvider mapperConfiguration)
    {
        _dbSet = context.Set<TEntity>();
        _mapperConfiguration = mapperConfiguration;
    }

    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object?>>[] includes)
    {
        return await ApplyIncludes(_dbSet.AsQueryable(), includes)
            .SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object?>>[] includes)
    {
        var query = ApplyIncludes(_dbSet.AsQueryable(), includes);

        if (predicate is not null)
            query = query.Where(predicate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TResponse?> ProjectToSingleOrDefaultAsync<TResponse>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ProjectTo<TResponse>(_mapperConfiguration)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TResponse>> ProjectToListAsync<TResponse>(Expression<Func<TEntity, object>> orderBy, Expression<Func<TEntity, bool>>? predicate = null, bool descending = false, CancellationToken cancellationToken = default) where TResponse : class
    {
        var query = _dbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        query = descending
                ? query.OrderByDescending(orderBy)
                : query.OrderBy(orderBy);

        return await query
            .ProjectTo<TResponse>(_mapperConfiguration)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<TResponse>> ProjectToPagedAsync<TResponse>(
        PaginationRequest pagination,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? predicate = null,
        bool descending = false,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        var query = _dbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);

        query = descending
            ? query.OrderByDescending(orderBy)
            : query.OrderBy(orderBy);

        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ProjectTo<TResponse>(_mapperConfiguration)
            .ToListAsync(cancellationToken);

        return new PagedResult<TResponse>(
            items,
            pagination.PageNumber,
            pagination.PageSize,
            totalCount);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    private static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, object?>>[] includes)
    {
        foreach (var include in includes)
            query = query.Include(include);

        return query;
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }
}
