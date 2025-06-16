using Microsoft.Extensions.Options;
using Task.PersonDirectory.Api;
using Task.PersonDirectory.Application;
using Task.PersonDirectory.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment)
    .AddApplication(builder.Configuration)
    .AddApi();

var app = builder.Build();
 
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseExceptionHandler();
app.MapHealthChecks("/health");
app.MapControllers();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.Run();