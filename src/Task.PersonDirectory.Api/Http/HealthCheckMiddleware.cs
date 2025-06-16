using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Api.Http;

public class DatabaseHealthCheckMiddleware(RequestDelegate next, ILogger<DatabaseHealthCheckMiddleware> logger)
{
    public async System.Threading.Tasks.Task InvokeAsync(HttpContext context, IDbContextFactory<PersonDirectoryContext> dbFactory)
    {
        if (context.Request.Path.ToString().EndsWith("/health"))
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var canConnect = await db.Database.CanConnectAsync();

                if (canConnect)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Healthy");
                }
                else
                {
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsync("Unhealthy");
                }

                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Error checking DB health");
                return;
            }
        }

        await next(context);
    }
}