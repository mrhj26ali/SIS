using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Domain;

namespace SIS.Infrastructure.Persistence.Configurations;

public class StudentCourseConfiguration : IEntityTypeConfiguration<StudentCourse>
{
    public void Configure(EntityTypeBuilder<StudentCourse> builder)
    {
        builder.ToTable("StudentCourses");
        
        builder.HasKey(sc => sc.Id);
        
        // Composite unique constraint: one student can enroll in a course only once
        builder.HasIndex(sc => new { sc.StudentId, sc.CourseId })
            .IsUnique()
            .HasDatabaseName("IX_StudentCourses_Student_Course");
        
        builder.Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(sc => sc.UpdatedAt)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(sc => sc.Student)
            .WithMany(s => s.StudentCourses)
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(sc => sc.Course)
            .WithMany(c => c.StudentCourses)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}