using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Infrastructure.Context;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Seed;

namespace Task.PersonDirectory.Infrastructure;

public static class Dependency
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddDbContextFactory<PersonDirectoryContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PersonDirectory"));
            if (!environment.IsDevelopment()) return;
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        });
        
        EnsureDatabase(services);
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<IRelatedPersonRepository, RelatedPersonRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        
        services.AddHealthChecks()
            .AddSqlServer(
                configuration.GetConnectionString("PersonDirectory")!,
                name: "SQL Server",
                timeout: TimeSpan.FromSeconds(5));
        
        return services;
    }

    private static void EnsureDatabase(IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        var context = sp.GetRequiredService<PersonDirectoryContext>();
        context.Database.EnsureCreated();
        CitySeeder.SeedAsync(context);
    }
}