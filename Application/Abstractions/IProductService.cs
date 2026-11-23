using Application.DTOs;
using Domain.Entities;

namespace Application.Abstractions;

public interface IProductService
{
    Task<Guid> CreateAsync(CreateProductRequest request);

    Task<Product> GetByIdAsync(Guid id);

    Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query);

    Task UpdateAsync(Guid id, CreateProductRequest request);
}
