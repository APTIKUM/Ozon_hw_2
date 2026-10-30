using Domain.Enums;

namespace Application.DTOs;

public sealed record GetProductsQuery(
    DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedTo = null,
    ProductType? Type = null,
    int? WarehouseNumber = null,
    int PageNumber = 1,
    int PageSize = 10
);