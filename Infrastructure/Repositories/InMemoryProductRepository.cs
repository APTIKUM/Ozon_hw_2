using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;

namespace Infrastructure.Repositories;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();
    private readonly Lock _sync = new();

    public Task<Guid> AddAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        lock (_sync)
        {
            _products.Add(product.Id, Copy(product));
        }

        return Task.FromResult(product.Id);
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        lock (_sync)
        {
            return Task.FromResult(_products.TryGetValue(id, out var product)
                ? Copy(product)
                : null);
        }
    }

    public Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(query.PageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(query.PageSize);

        Product[] snapshot;
        lock (_sync)
        {
            snapshot = _products.Values.ToArray();
        }

        IEnumerable<Product> filtered = snapshot;

        if (query.CreatedFrom.HasValue)
        {
            filtered = filtered.Where(product => product.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            filtered = filtered.Where(product => product.CreatedAt <= query.CreatedTo.Value);
        }

        if (query.Type.HasValue)
        {
            filtered = filtered.Where(product => product.Type == query.Type.Value);
        }

        if (query.WarehouseNumber.HasValue)
        {
            filtered = filtered.Where(product => product.WarehouseNumber == query.WarehouseNumber.Value);
        }

        var matches = filtered.ToArray();
        var offset = (long)(query.PageNumber - 1) * query.PageSize;
        var items = offset >= matches.Length
            ? Array.Empty<Product>()
            : matches.OrderBy(product => product.CreatedAt)
                .ThenBy(product => product.Id)
                .Skip((int)offset)
                .Take(query.PageSize)
                .Select(Copy)
                .ToArray();

        return Task.FromResult(new PagedResult<Product>(
            items, matches.Length, query.PageNumber, query.PageSize));
    }

    private static Product Copy(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        Weight = product.Weight,
        Type = product.Type,
        CreatedAt = product.CreatedAt,
        WarehouseNumber = product.WarehouseNumber
    };
}
