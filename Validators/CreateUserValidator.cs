using FluentValidation;
using DigitalSignage.DTOs;

namespace DigitalSignage.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDTO>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required")
                .MaximumLength(100).WithMessage("User name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("User name can only contain letters, numbers, hyphens, and underscores");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .When(x => !x.IsOffice365User);

            RuleFor(x => x.FirstName)
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.AzureADObjectId)
                .NotEmpty().WithMessage("Azure AD Object ID is required for Office 365 users")
                .When(x => x.IsOffice365User);
        }
    }
}
