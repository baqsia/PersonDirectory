using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Task.PersonDirectory.Infrastructure.Context;

public class DesignTimePersonDirectoryContextFactory : IDesignTimeDbContextFactory<PersonDirectoryContext>
{
    public PersonDirectoryContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // important for CLI
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PersonDirectoryContext>();
        var connectionString = config.GetConnectionString("PersonDirectory");

        optionsBuilder.UseSqlServer(connectionString);

        return new PersonDirectoryContext(optionsBuilder.Options);
    }
}