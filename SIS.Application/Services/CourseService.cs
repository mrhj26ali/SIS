using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
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

    public CourseService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCourseDto> createValidator,
        IValidator<UpdateCourseDto> updateValidator,
        ILogger<CourseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
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

    public async Task<int> CreateAsync(CreateCourseDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var course = _mapper.Map<Course>(dto);
        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.CompleteAsync();
        return course.Id;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCourseDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid) throw new ValidationException(validation.Errors);

        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) throw new NotFoundException(nameof(Course), id);

        if (!string.IsNullOrWhiteSpace(dto.Name)) course.Name = dto.Name;
        if (dto.Description != null) course.Description = dto.Description;
        course.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Courses.Update(course);
        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(id);
        if (course == null) throw new NotFoundException(nameof(Course), id);

        // Prevent deletion if students are enrolled
        if (course.StudentCourses.Any())
            throw new ConflictException("Cannot delete a course with active enrollments.");

        _unitOfWork.Courses.Delete(course);
        return await _unitOfWork.CompleteAsync() > 0;
    }
}