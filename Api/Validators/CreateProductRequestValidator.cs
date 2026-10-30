using Application.DTOs;
using FluentValidation;

namespace Api.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty() .MaximumLength(200);

        RuleFor(x => x.Price).GreaterThan(0);

        RuleFor(x => x.Weight).GreaterThan(0);

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.WarehouseNumber).GreaterThan(0);
    }
}
