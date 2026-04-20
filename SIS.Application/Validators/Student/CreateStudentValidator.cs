using FluentValidation;
using SIS.Application.DTOs.Student;

namespace SIS.Application.Validators.Student;

public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StudentNumber)
            .NotEmpty()
            .Matches(@"^STU-\d{6}$").WithMessage("Format: STU-123456")
            .MaximumLength(20);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.Password).MinimumLength(6);
        RuleFor(x => x.Age).InclusiveBetween(16, 100);
    }
}