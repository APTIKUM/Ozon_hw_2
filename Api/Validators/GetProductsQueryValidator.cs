using Application.DTOs;
using FluentValidation;

namespace Api.Validators;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);

        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.Type).IsInEnum()
            .When(x => x.Type.HasValue);

        RuleFor(x => x.WarehouseNumber).GreaterThan(0)
            .When(x => x.WarehouseNumber.HasValue);

        RuleFor(x => x.CreatedTo).GreaterThanOrEqualTo(x => x.CreatedFrom)
            .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue);
    }
}
