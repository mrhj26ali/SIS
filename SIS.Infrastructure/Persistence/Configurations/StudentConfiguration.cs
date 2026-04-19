using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIS.Domain;

namespace SIS.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        
        // Primary Key
        builder.HasKey(s => s.Id);
        
        // Properties with constraints
        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.HasIndex(s => s.StudentNumber)
            .IsUnique()
            .HasDatabaseName("IX_Students_StudentNumber");
            
        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(s => s.Password)
            .IsRequired(); // Will store hashed password
            
        builder.Property(s => s.Age)
            .IsRequired();
            
        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);
            
        // Timestamps
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(s => s.UpdatedAt)
            .IsRequired(false);
        
        // Navigation: Student -> StudentCourses
        builder.HasMany(s => s.StudentCourses)
            .WithOne(sc => sc.Student)
            .HasForeignKey(sc => sc.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}