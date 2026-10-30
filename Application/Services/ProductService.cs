using Application.Abstractions;
using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;

namespace Application.Services;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    private readonly IProductRepository _repository = repository;

    public Task<Guid> CreateAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTimeOffset.UtcNow;

        return _repository.AddAsync(product);
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);

        return product ?? throw new ProductNotFoundException(id);
    }

    public Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query)
    {
        return _repository.GetPagedAsync(query);
    }
}