using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductService service,
    IValidator<CreateProductRequest> createValidator,
    IValidator<GetProductsQuery> queryValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var id = await service.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> GetById(Guid id)
    {
        return Ok(await service.GetByIdAsync(id));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, CreateProductRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await service.UpdateAsync(id, request);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<Product>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<Product>>> GetProducts(
        [FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
    {
        await queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        return Ok(await service.GetPagedAsync(query));
    }
}
