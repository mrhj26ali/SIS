using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Logging.Infrastructure.Contexts;

public class LoggingDbContextFactory : IDesignTimeDbContextFactory<LoggingDbContext>
{
    public LoggingDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Logging.API"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();

        // Build DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<LoggingDbContext>();
        optionsBuilder.UseSqlServer(
            configuration.GetConnectionString("Default"),
            sql => sql.MigrationsAssembly(typeof(LoggingDbContext).Assembly.FullName));

        return new LoggingDbContext(optionsBuilder.Options);
    }
}