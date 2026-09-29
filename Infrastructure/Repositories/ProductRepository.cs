using Api.Contracts.Common;
using Application.Abstractions.Repositories;
using Application.Common.Filters;
using Application.Common.Pagination;
using Application.Features.Products.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Catalog;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Extensions;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly DbSet<Product> _dbSet;
    private readonly IConfigurationProvider _mapperConfiguration;

    public ProductRepository(ApplicationDbContext context,
        IConfigurationProvider mapperConfiguration) : base(context, mapperConfiguration)
    {
        _dbSet = context.Set<Product>();
        _mapperConfiguration = mapperConfiguration;
    }

    public async Task<PagedResult<ProductInList>> ProjectToPagedWithFiltersAsync(
        PaginationRequest? pagination,
        ProductFilter? filters,
        CancellationToken cancellationToken = default)
    {
        pagination ??= new PaginationRequest();

        var query = _dbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .ApplyFilters(filters)
            .ApplySorting(filters);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ProjectTo<ProductInList>(_mapperConfiguration)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductInList>(
            items,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }
}
