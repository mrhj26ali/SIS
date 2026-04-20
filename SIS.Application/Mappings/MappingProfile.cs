using AutoMapper;
using SIS.Application.DTOs.Course;
using SIS.Application.DTOs.Student;
using SIS.Domain;

namespace SIS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Student Mappings
        CreateMap<CreateStudentDto, Student>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StudentCourses, opt => opt.Ignore());

        CreateMap<UpdateStudentDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StudentNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StudentCourses, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        CreateMap<Student, StudentListDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(s => s.FullName))
            .ForMember(dest => dest.CourseNames, opt => 
                opt.MapFrom(s => s.StudentCourses.Select(sc => sc.Course.Name).ToList()));

        CreateMap<Student, StudentDetailDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(s => s.FullName))
            .ForMember(dest => dest.Courses, opt => 
                opt.MapFrom(s => s.StudentCourses.Select(sc => new CourseInfoDto 
                { 
                    Id = sc.Course.Id, 
                    Name = sc.Course.Name, 
                    Description = sc.Course.Description 
                })));

        // Course Mappings
        CreateMap<CreateCourseDto, Course>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StudentCourses, opt => opt.Ignore());

        CreateMap<UpdateCourseDto, Course>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.StudentCourses, opt => opt.Ignore());

        CreateMap<Course, CourseListDto>()
            .ForMember(dest => dest.EnrolledStudentsCount, opt => 
                opt.MapFrom(c => c.StudentCourses.Count));
    }
}