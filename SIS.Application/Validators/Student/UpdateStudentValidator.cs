using FluentValidation;
using SIS.Application.DTOs.Student;

namespace SIS.Application.Validators.Student;

public class UpdateStudentValidator : AbstractValidator<UpdateStudentDto>
{
    public UpdateStudentValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.FirstName));
        RuleFor(x => x.LastName).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.LastName));
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        RuleFor(x => x.Age).InclusiveBetween(16, 100).When(x => x.Age > 0);
    }
}