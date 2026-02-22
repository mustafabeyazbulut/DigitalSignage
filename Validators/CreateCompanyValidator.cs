using FluentValidation;
using DigitalSignage.Models;

namespace DigitalSignage.Validators
{
    public class CreateCompanyValidator : AbstractValidator<Company>
    {
        public CreateCompanyValidator()
        {
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(255).WithMessage("Company name cannot exceed 255 characters");

            RuleFor(x => x.EmailDomain)
                .MaximumLength(255).WithMessage("Email domain cannot exceed 255 characters")
                .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Invalid email domain format")
                .When(x => !string.IsNullOrEmpty(x.EmailDomain));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        }
    }
}
