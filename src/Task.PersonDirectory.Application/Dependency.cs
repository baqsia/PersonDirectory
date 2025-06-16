using FluentValidation;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nest;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.Common.ValidationPipeline;
using Task.PersonDirectory.Application.HealthChecks;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Application;

public static class Dependency
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        services.AddMediator(opt =>
        {
            opt.ServiceLifetime = ServiceLifetime.Scoped;
            opt.Namespace = "Task.PersonDirectory.Application";
        });
        services.AddValidatorsFromAssembly(AssemblyMarker.Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        services.AddSingleton<IElasticClient>(_ =>
        {
            var settings = new ConnectionSettings(new Uri(configuration.GetConnectionString("Elasticsearch")!))
                .DefaultIndex("persons")
                .EnableDebugMode();

            return new ElasticClient(settings);
        });

        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();
        services.AddScoped<IPersonSearchIndexer, PersonSearchIndexer>();

        services.AddScoped<IElasticStatusChecker, ElasticStatusChecker>();
        services.AddHealthChecks()
            .AddCheck<ElasticHealthCheck>("Elastic HealthCheck");
        return services;
    }
}