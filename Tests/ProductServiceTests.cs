using Application.Abstractions;
using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

public sealed class ProductServiceTests
{
    private static readonly CreateProductRequest Request = new("Product", 12.34m, 0.125m, ProductType.General, 17);

    [Test]
    public async Task CreateReturnsRepositoryIdAndPersistsRequestedValues()
    {
        var repository = new StubRepository();
        var service = new ProductService(repository);

        var id = await service.CreateAsync(Request);
        var saved = repository.Saved.Should().BeOfType<Product>().Which;

        var createProductRequest =
            new CreateProductRequest(saved.Name, saved.Price, saved.Weight, saved.Type, saved.WarehouseNumber);

        id.Should().Be(repository.CreatedId);
        createProductRequest.Should().Be(Request);
        saved.Id.Should().NotBeEmpty();
        saved.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public async Task GetByIdThrowsWhenProductIsMissing()
    {
        var service = new ProductService(new StubRepository());

        var getProduct = () => service.GetByIdAsync(Guid.NewGuid());

        await getProduct.Should().ThrowExactlyAsync<ProductNotFoundException>();
    }

    [Test]
    public async Task UpdateThrowsWhenProductIsMissing()
    {
        var service = new ProductService(new StubRepository());

        var updateProduct = () => service.UpdateAsync(Guid.NewGuid(), Request);

        await updateProduct.Should().ThrowExactlyAsync<ProductNotFoundException>();
    }

    private sealed class StubRepository : IProductRepository
    {
        public Guid CreatedId { get; } = Guid.NewGuid();
        public Product? Saved { get; private set; }

        public Task<Guid> AddAsync(Product product)
        {
            Saved = product;
            return Task.FromResult(CreatedId);
        }

        public Task<Product?> GetByIdAsync(Guid id) => Task.FromResult<Product?>(null);

        public Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query) => throw new NotSupportedException();

        public Task<bool> UpdateAsync(Guid id, CreateProductRequest request) => Task.FromResult(false);
    }
}
