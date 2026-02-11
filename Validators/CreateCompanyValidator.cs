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

            RuleFor(x => x.CompanyCode)
                .NotEmpty().WithMessage("Company code is required")
                .MaximumLength(50).WithMessage("Company code cannot exceed 50 characters")
                .Matches(@"^[A-Z0-9_-]+$").WithMessage("Company code must contain only uppercase letters, numbers, hyphens, and underscores");

            RuleFor(x => x.EmailDomain)
                .MaximumLength(255).WithMessage("Email domain cannot exceed 255 characters")
                .Matches(@"^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Invalid email domain format")
                .When(x => !string.IsNullOrEmpty(x.EmailDomain));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.PrimaryColor)
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Invalid color format. Use hex format (e.g., #FFFFFF)")
                .When(x => !string.IsNullOrEmpty(x.PrimaryColor));

            RuleFor(x => x.SecondaryColor)
                .Matches(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Invalid color format. Use hex format (e.g., #FFFFFF)")
                .When(x => !string.IsNullOrEmpty(x.SecondaryColor));
        }
    }
}
