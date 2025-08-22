using Engrslan.Application.Contracts.Dtos;
using FluentValidation;

namespace Engrslan.Application.Contracts.Validators;

public class UpdateSampleDtoValidator : AbstractValidator<UpdateSampleDto>
{
    public UpdateSampleDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0")
            .PrecisionScale(18, 2, true).WithMessage("Price must have no more than 2 decimal places");
    }
}