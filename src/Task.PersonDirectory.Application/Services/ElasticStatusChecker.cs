using Elasticsearch.Net;
using Nest;

namespace Task.PersonDirectory.Application.Services;

public interface IElasticStatusChecker
{
    Task<Health> GetHealthStatusAsync(CancellationToken cancellationToken);
}

public class ElasticStatusChecker(IElasticClient client) : IElasticStatusChecker
{
    public async Task<Health> GetHealthStatusAsync(CancellationToken cancellationToken)
    {
        var health = await client.Cluster.HealthAsync(ct: cancellationToken);
        return health.Status;
    }
}