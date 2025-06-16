using Microsoft.EntityFrameworkCore;

namespace Task.PersonDirectory.Infrastructure.Context;

public class PersonDirectoryContext(DbContextOptions<PersonDirectoryContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyMarker.Assembly);
    }
}