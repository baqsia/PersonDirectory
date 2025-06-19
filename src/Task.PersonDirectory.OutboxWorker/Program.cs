using Nest;
using Task.PersonDirectory.Infrastructure;
using Task.PersonDirectory.OutboxWorker;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddSingleton<IElasticClient>(_ =>
{
    var settings = new ConnectionSettings(new Uri(builder.Configuration.GetConnectionString("Elasticsearch")!))
        .DefaultIndex("persons")
        .EnableDebugMode();

    return new ElasticClient(settings);
});
builder.Services.AddTransient<IPersonSearchIndexer, PersonSearchIndexer>();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddHostedService<OutboxProcessor>();

var app = builder.Build();

app.Run();
