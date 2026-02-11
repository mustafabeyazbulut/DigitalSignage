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
                .Must(BeAValidContentType).WithMessage("Content type must be one of: Text, Image, Video, HTML");

            RuleFor(x => x.ContentTitle)
                .MaximumLength(255).WithMessage("Content title cannot exceed 255 characters");

            RuleFor(x => x.ContentData)
                .NotEmpty().WithMessage("Content data is required")
                .MaximumLength(50000).WithMessage("Content data cannot exceed 50000 characters");

            RuleFor(x => x.MediaPath)
                .MaximumLength(500).WithMessage("Media path cannot exceed 500 characters");

            RuleFor(x => x.DurationSeconds)
                .GreaterThan(0).WithMessage("Duration must be greater than 0")
                .LessThanOrEqualTo(3600).WithMessage("Duration cannot exceed 3600 seconds (1 hour)")
                .When(x => x.DurationSeconds.HasValue);
        }

        private bool BeAValidContentType(string contentType)
        {
            var validTypes = new[] { "Text", "Image", "Video", "HTML" };
            return validTypes.Contains(contentType);
        }
    }
}
