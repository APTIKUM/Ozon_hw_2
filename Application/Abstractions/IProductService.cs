using Application.DTOs;
using Domain.Entities;

namespace Application.Abstractions;

public interface IProductService
{
    public Task<Guid> CreateAsync(CreateProductRequest request);
    public Task<Product> GetByIdAsync(Guid id);
    public Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query);

}