using Application.Abstractions;
using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;

namespace Application.Services;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    public Task<Guid> CreateAsync(CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Weight = request.Weight,
            Type = request.Type,
            CreatedAt = DateTimeOffset.UtcNow,
            WarehouseNumber = request.WarehouseNumber
        };

        return repository.AddAsync(entity);
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var product = await repository.GetByIdAsync(id);

        return product ?? throw new ProductNotFoundException(id);
    }

    public Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query)
    {
        return repository.GetPagedAsync(query);
    }
}
