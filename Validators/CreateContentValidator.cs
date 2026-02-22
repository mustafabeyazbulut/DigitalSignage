using FluentValidation;
using DigitalSignage.DTOs;

namespace DigitalSignage.Validators
{
    public class CreateContentValidator : AbstractValidator<CreateContentDTO>
    {
        public CreateContentValidator()
        {
            RuleFor(x => x.DepartmentID)
                .GreaterThan(0).WithMessage("Department is required");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("Content type is required")
                .Must(BeAValidContentType).WithMessage("Content type must be one of: Text, Image, Video, HTML, PDF, URL");

            RuleFor(x => x.ContentTitle)
                .MaximumLength(255).WithMessage("Content title cannot exceed 255 characters");

            RuleFor(x => x.ContentData)
                .MaximumLength(50000).WithMessage("Content data cannot exceed 50000 characters")
                .When(x => !string.IsNullOrEmpty(x.ContentData));

            RuleFor(x => x.MediaPath)
                .MaximumLength(500).WithMessage("Media path cannot exceed 500 characters");
        }

        private bool BeAValidContentType(string contentType)
        {
            var validTypes = new[] { "Text", "Image", "Video", "HTML", "PDF", "URL" };
            return validTypes.Contains(contentType);
        }
    }
}
