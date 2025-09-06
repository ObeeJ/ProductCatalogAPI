using FluentValidation;
using ProductCatalogAPI.Application.DTOs;

namespace ProductCatalogAPI.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.OrderItems)
            .SetValidator(new CreateOrderItemValidator());
    }
}

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}