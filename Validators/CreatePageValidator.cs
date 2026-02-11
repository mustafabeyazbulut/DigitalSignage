using FluentValidation;
using DigitalSignage.DTOs;

namespace DigitalSignage.Validators
{
    public class CreatePageValidator : AbstractValidator<CreatePageDTO>
    {
        public CreatePageValidator()
        {
            RuleFor(x => x.DepartmentID)
                .GreaterThan(0).WithMessage("Department is required");

            RuleFor(x => x.PageName)
                .NotEmpty().WithMessage("Page name is required")
                .MaximumLength(255).WithMessage("Page name cannot exceed 255 characters");

            RuleFor(x => x.PageTitle)
                .NotEmpty().WithMessage("Page title is required")
                .MaximumLength(255).WithMessage("Page title cannot exceed 255 characters");

            RuleFor(x => x.LayoutID)
                .GreaterThan(0).WithMessage("Layout is required");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
        }
    }
}
