using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MassTransit;
using SIS.Contracts;
using SIS.Application.DTOs.Course;
using SIS.Application.Exceptions;
using SIS.Application.Interfaces;
using SIS.Domain;
using SIS.Domain.Common.Interfaces;

namespace SIS.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCourseDto> _createValidator;
    private readonly IValidator<UpdateCourseDto> _updateValidator;
    private readonly ILogger<CourseService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public CourseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCourseDto> createValidator,
        IValidator<UpdateCourseDto> updateValidator,
        ILogger<CourseService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<IEnumerable<CourseListDto>> GetAllAsync()
    {
        var courses = await _unitOfWork.Courses.GetAllAsync();
        return _mapper.Map<IEnumerable<CourseListDto>>(courses);
    }

    public async Task<CourseListDto> GetByIdAsync(int id)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) throw new NotFoundException(nameof(Course), id);
        return _mapper.Map<CourseListDto>(course);
    }

    public async Task<int> CreateAsync(CreateCourseDto dto, string? identityUserId = null)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);
        var course = _mapper.Map<Course>(dto);
        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.CompleteAsync();
        await _publishEndpoint.Publish(new LogMessage($"Created course {course.Id}", BuildCreatedBy(identityUserId)));
        return course.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCourseDto dto, string? identityUserId = null)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) throw new NotFoundException(nameof(Course), id);
        if (!string.IsNullOrWhiteSpace(dto.Name)) course.Name = dto.Name;
        if (dto.Description != null) course.Description = dto.Description;
        course.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Courses.Update(course);
        await _publishEndpoint.Publish(new LogMessage($"Updated course {id}", BuildCreatedBy(identityUserId)));

        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id, string? identityUserId = null)
    {
        var course = await _unitOfWork.Courses.GetCourseWithEnrollmentsAsync(id);
        if (course == null) throw new NotFoundException(nameof(Course), id);
        if (course.StudentCourses.Any()) throw new ConflictException("Cannot delete a course with active enrollments.");
        _unitOfWork.Courses.Delete(course);
        await _publishEndpoint.Publish(new LogMessage($"Deleted course {id}", BuildCreatedBy(identityUserId)));

        return await _unitOfWork.CompleteAsync() > 0;
    }

    private static string BuildCreatedBy(string? identityUserId = null)
    {
        return string.IsNullOrWhiteSpace(identityUserId)
            ? "system - CourseService"
            : $"user {identityUserId} - CourseService";
    }
}