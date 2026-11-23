using System.Data;
using Application.Abstractions;
using Application.DTOs;
using Dapper;
using Domain.Entities;
using Npgsql;

namespace Infrastructure.Repositories;

public sealed class PostgresProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{
    private const string Columns = """
        id, name, price, weight, type, created_at AS "CreatedAt", warehouse_number AS "WarehouseNumber"
        """;

    public async Task<Guid> AddAsync(Product product)
    {
        const string sql = """
            INSERT INTO products (id, name, price, weight, type, created_at, warehouse_number)
            VALUES (@Id, @Name, @Price, @Weight, @Type, @CreatedAt, @WarehouseNumber)
            RETURNING id
            """;

        await using var connection = await dataSource.OpenConnectionAsync();
        return await connection.QuerySingleAsync<Guid>(sql, new
        {
            product.Id,
            product.Name,
            product.Price,
            product.Weight,
            Type = (int)product.Type,
            CreatedAt = product.CreatedAt.UtcDateTime,
            product.WarehouseNumber
        });
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Product>(
            $"SELECT {Columns} FROM products WHERE id = @Id", new { Id = id });
    }

    public async Task<PagedResult<Product>> GetPagedAsync(GetProductsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(query.PageNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(query.PageSize);

        const string filter = """
            WHERE (CAST(@CreatedFrom AS timestamptz) IS NULL OR created_at >= @CreatedFrom)
              AND (CAST(@CreatedTo AS timestamptz) IS NULL OR created_at <= @CreatedTo)
              AND (CAST(@Type AS integer) IS NULL OR type = @Type)
              AND (CAST(@WarehouseNumber AS integer) IS NULL OR warehouse_number = @WarehouseNumber)
            """;
        const string sql = $"""
            SELECT count(*) FROM products {filter};
            SELECT {Columns} FROM products {filter}
            ORDER BY created_at, id
            LIMIT @PageSize OFFSET @Offset;
            """;
        var parameters = new
        {
            CreatedFrom = query.CreatedFrom?.UtcDateTime,
            CreatedTo = query.CreatedTo?.UtcDateTime,
            Type = (int?)query.Type,
            query.WarehouseNumber,
            query.PageSize,
            Offset = (long)(query.PageNumber - 1) * query.PageSize
        };

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);
        await using var results = await connection.QueryMultipleAsync(sql, parameters, transaction);
        var count = await results.ReadSingleAsync<int>();
        var items = (await results.ReadAsync<Product>()).AsList();
        await transaction.CommitAsync();

        return new PagedResult<Product>(items, count, query.PageNumber, query.PageSize);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateProductRequest request)
    {
        const string sql = """
            UPDATE products
            SET name = @Name, price = @Price, weight = @Weight, type = @Type,
                warehouse_number = @WarehouseNumber
            WHERE id = @Id
            """;

        await using var connection = await dataSource.OpenConnectionAsync();
        return await connection.ExecuteAsync(sql, new
        {
            Id = id,
            request.Name,
            request.Price,
            request.Weight,
            Type = (int)request.Type,
            request.WarehouseNumber
        }) == 1;
    }
}
