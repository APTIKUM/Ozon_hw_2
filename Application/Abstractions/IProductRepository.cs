using Domain.Entities;
using Application.DTOs;

namespace Application.Abstractions;

public interface IProductRepository
{
    Task<Guid> AddAsync(Product product);

    Task<Product?> GetByIdAsync(Guid id);

    Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query);

    Task<bool> UpdateAsync(Guid id, CreateProductRequest request);
}
