using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;
using Task.PersonDirectory.Api.Http;
using Task.PersonDirectory.Api.Resources;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Api;

public static class Dependency
{
    public static void AddApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        services.AddOpenApi()
            .AddProblemDetails(cfg =>
            {
                cfg.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance =
                        $"{context.HttpContext.Request.Method} - {context.HttpContext.Request.Path}";
                };
            })
            .AddHttpContextAccessor()
            .AddSingleton<IImageStorage, FileSystemImageStorage>(sp =>
            {
                var webHostEnvironment = sp.GetRequiredService<IWebHostEnvironment>();
                return new FileSystemImageStorage(webHostEnvironment.ContentRootPath);
            });

        services.AddExceptionHandler<DefaultExceptionHandler>();
        services.AddHostedService<OutboxProcessor>();
        
        services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("ka")
            };

            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders = [ new AcceptLanguageHeaderRequestCultureProvider() ];
        });
    }
}