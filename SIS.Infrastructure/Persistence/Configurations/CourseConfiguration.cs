using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Domain;

namespace SIS.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(c => c.Description)
            .HasMaxLength(1000);
            
        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);
        
        // Navigation: Course -> StudentCourses
        builder.HasMany(c => c.StudentCourses)
            .WithOne(sc => sc.Course)
            .HasForeignKey(sc => sc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}