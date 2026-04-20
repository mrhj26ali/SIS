using FluentValidation;
using SIS.Application.DTOs.Course;

namespace SIS.Application.Validators.Course;

public class UpdateCourseValidator : AbstractValidator<UpdateCourseDto>
{
    public UpdateCourseValidator()
    {
        // Name is optional on update, but if provided, check length
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Course name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));
            
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}