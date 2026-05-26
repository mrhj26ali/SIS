using AutoMapper;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using SIS.Application.DTOs.Student;
using SIS.Application.Exceptions;
using SIS.Application.Interfaces;
using SIS.Contracts;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;

namespace SIS.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateStudentDto> _createValidator;
    private readonly IValidator<UpdateStudentDto> _updateValidator;
    private readonly ILogger<StudentService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public StudentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateStudentDto> createValidator,
        IValidator<UpdateStudentDto> updateValidator,
        ILogger<StudentService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<StudentListDto>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all students");
        var students = await _unitOfWork.Students.GetAllAsync();
        return _mapper.Map<IEnumerable<StudentListDto>>(students);
    }

    public async Task<StudentDetailDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching student with ID {Id}", id);
        var student = await _unitOfWork.Students.GetStudentWithCoursesAsync(id);
        if (student == null) throw new NotFoundException(nameof(Student), id);
        return _mapper.Map<StudentDetailDto>(student);
    }

    public async Task<int> CreateAsync(CreateStudentDto dto, string? identityUserId = null)
    {
        _logger.LogInformation("Creating student with number {Number}", dto.StudentNumber);

        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        if (await StudentNumberExistsAsync(dto.StudentNumber))
            throw new ConflictException("Student number already exists.");

        var student = _mapper.Map<Student>(dto);
        student.IdentityUserId = identityUserId;

        await _unitOfWork.Students.AddAsync(student);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Student created with ID {Id}", student.Id);
        await _publishEndpoint.Publish(new LogMessage
        {
            Message = $"Student {student.Id} created",
            CreatedBy = BuildCreatedBy("StudentService", identityUserId)
        });

        return student.Id;
    }

    public async Task<bool> StudentNumberExistsAsync(string studentNumber)
    {
        return await _unitOfWork.Students.GetByStudentNumberAsync(studentNumber) != null;
    }

    public async Task<StudentDetailDto> GetByIdentityUserIdAsync(string identityUserId)
    {
        var student = await _unitOfWork.Students.GetByIdentityUserIdAsync(identityUserId);
        if (student == null) throw new NotFoundException(nameof(Student), identityUserId);
        return _mapper.Map<StudentDetailDto>(student);
    }

    public async Task<bool> UpdateAsync(int id, UpdateStudentDto dto, string? identityUserId = null)
    {
        _logger.LogInformation("Updating student {Id}", id);
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var student = await _unitOfWork.Students.GetByIdAsync(id);
        if (student == null) throw new NotFoundException(nameof(Student), id);

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) student.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) student.LastName = dto.LastName;
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) student.PhoneNumber = dto.PhoneNumber;
        if (dto.Age > 0) student.Age = dto.Age;
        student.IsActive = dto.IsActive;
        student.UpdatedAt = DateTime.UtcNow;

        if (dto.CourseIds != null && dto.CourseIds.Any())
        {
            await SyncStudentCoursesAsync(student, dto.CourseIds);
        }

        _unitOfWork.Students.Update(student);
        var result = await _unitOfWork.CompleteAsync() > 0;
        
        if (result)
        {
            await _publishEndpoint.Publish(new LogMessage
            {
                Message = $"Student {id} updated",
                CreatedBy = BuildCreatedBy("StudentService", identityUserId)
            });
        }
        
        return result;
    }

    public async Task<bool> DeleteAsync(int id, string? identityUserId = null)
    {
        _logger.LogInformation("Soft-deleting student {Id}", id);
        var student = await _unitOfWork.Students.GetByIdAsync(id);
        if (student == null) throw new NotFoundException(nameof(Student), id);

        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Students.Update(student);
        var result = await _unitOfWork.CompleteAsync() > 0;
        
        if (result)
        {
            await _publishEndpoint.Publish(new LogMessage
            {
                Message = $"Student {id} deleted",
                CreatedBy = BuildCreatedBy("StudentService", identityUserId)
            });
        }
        
        return result;
    }

    public async Task<bool> EnrollInCourseAsync(int studentId, int courseId, string? identityUserId = null)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null || !student.IsActive) throw new NotFoundException(nameof(Student), studentId);

        var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
        if (course == null) throw new NotFoundException(nameof(Course), courseId);

        if (await _unitOfWork.StudentCourses.IsEnrolledAsync(studentId, courseId))
            throw new ConflictException("Student is already enrolled in this course.");

        var enrollment = new StudentCourse { StudentId = studentId, CourseId = courseId };
        await _unitOfWork.StudentCourses.AddAsync(enrollment);
        var result = await _unitOfWork.CompleteAsync() > 0;
        
        if (result)
        {
            await _publishEndpoint.Publish(new LogMessage
            {
                Message = $"Student {studentId} enrolled in course {courseId}",
                CreatedBy = BuildCreatedBy("StudentService", identityUserId)
            });
        }

        return result;
    }

    public async Task<bool> UnenrollFromCourseAsync(int studentId, int courseId, string? identityUserId = null)
    {
        var enrollment = await _unitOfWork.StudentCourses.GetEnrollmentAsync(studentId, courseId);
        if (enrollment == null) throw new NotFoundException("Enrollment", $"{studentId}-{courseId}");

        _unitOfWork.StudentCourses.Delete(enrollment);
        var result = await _unitOfWork.CompleteAsync() > 0;
        
        if (result)
        {
            await _publishEndpoint.Publish(new LogMessage
            {
                Message = $"Student {studentId} unenrolled from course {courseId}",
                CreatedBy = BuildCreatedBy("StudentService", identityUserId)
            });
        }

        return result;
    }

    private static string BuildCreatedBy(string serviceName, string? identityUserId)
    {
        return string.IsNullOrWhiteSpace(identityUserId)
            ? $"system - {serviceName}"
            : $"user {identityUserId} - {serviceName}";
    }

    private async Task SyncStudentCoursesAsync(Student student, List<int> newCourseIds)
    {
        var currentIds = student.StudentCourses.Select(sc => sc.CourseId).ToList();
        var toAdd = newCourseIds.Except(currentIds);
        var toRemove = student.StudentCourses.Where(sc => !newCourseIds.Contains(sc.CourseId)).ToList();

        foreach (var courseId in toAdd)
        {
            if (await _unitOfWork.Courses.GetByIdAsync(courseId) != null)
            {
                student.StudentCourses.Add(new StudentCourse { StudentId = student.Id, CourseId = courseId });
            }
        }

        foreach (var enrollment in toRemove)
            student.StudentCourses.Remove(enrollment);
    }
}