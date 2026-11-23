using System.Globalization;
using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;
using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using CreateProductDto = Application.DTOs.CreateProductRequest;
using DomainProductType = Domain.Enums.ProductType;

namespace Api.Grpc;

public sealed class ProductsGrpcService(
    IProductService service,
    IValidator<CreateProductDto> createValidator,
    IValidator<GetProductsQuery> queryValidator) : Products.ProductsBase
{
    public override async Task<CreateProductResponse> CreateProduct(
        CreateProductRequest request,
        ServerCallContext context)
    {
        var dto = ToDto(request);
        await createValidator.ValidateAndThrowAsync(dto, context.CancellationToken);
        var id = await service.CreateAsync(dto);

        return new CreateProductResponse { Id = id.ToString() };
    }

    public override async Task<ProductResponse> GetProductById(
        GetProductByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw InvalidArgument("id must be a valid GUID.");
        }

        var product = await service.GetByIdAsync(id);
        return ToResponse(product);
    }

    public override async Task<Empty> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
        {
            throw InvalidArgument("id must be a valid GUID.");
        }

        var product = request.Product ?? throw InvalidArgument("product is required.");
        var dto = ToDto(product);
        await createValidator.ValidateAndThrowAsync(dto, context.CancellationToken);
        await service.UpdateAsync(id, dto);

        return new Empty();
    }

    public override async Task<GetProductsResponse> GetProducts(
        GetProductsRequest request,
        ServerCallContext context)
    {
        var query = new GetProductsQuery(
            ParseTimestamp(request.CreatedFrom, "created_from"),
            ParseTimestamp(request.CreatedTo, "created_to"),
            request.HasType ? (DomainProductType)request.Type : null,
            request.HasWarehouseNumber ? request.WarehouseNumber : null,
            request.HasPageNumber ? request.PageNumber : 1,
            request.HasPageSize ? request.PageSize : 10);

        await queryValidator.ValidateAndThrowAsync(query, context.CancellationToken);
        var result = await service.GetPagedAsync(query);

        var response = new GetProductsResponse
        {
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };

        foreach (var product in result.Items)
        {
            response.Items.Add(ToResponse(product));
        }

        return response;
    }

    private static CreateProductDto ToDto(CreateProductRequest request) => new(
        request.Name,
        ParseDecimal(request.Price, "price"),
        ParseDecimal(request.Weight, "weight"),
        (DomainProductType)request.Type,
        request.WarehouseNumber);

    private static ProductResponse ToResponse(Product product) => new()
    {
        Id = product.Id.ToString(),
        Name = product.Name,
        Price = product.Price.ToString(CultureInfo.InvariantCulture),
        Weight = product.Weight.ToString(CultureInfo.InvariantCulture),
        Type = (ProductType)product.Type,
        CreatedAt = Timestamp.FromDateTimeOffset(product.CreatedAt),
        WarehouseNumber = product.WarehouseNumber
    };

    private static decimal ParseDecimal(string value, string fieldName)
    {
        const NumberStyles styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (!decimal.TryParse(value, styles, CultureInfo.InvariantCulture, out var result))
        {
            throw InvalidArgument($"{fieldName} must be a decimal number with a dot as the decimal separator.");
        }

        return result;
    }

    private static DateTimeOffset? ParseTimestamp(Timestamp? value, string fieldName)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            return value.ToDateTimeOffset();
        }
        catch (InvalidOperationException)
        {
            throw InvalidArgument($"{fieldName} must be a valid timestamp.");
        }
    }

    private static RpcException InvalidArgument(string message) =>
        new(new Status(StatusCode.InvalidArgument, message));
}
