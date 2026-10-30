using Domain.Enums;

namespace Application.DTOs;

public sealed record CreateProductRequest(
    string Name,
    decimal Price,
    decimal Weight,
    ProductType Type,
    int WarehouseNumber
);
