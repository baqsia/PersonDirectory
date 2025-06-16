using Elasticsearch.Net;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Application.HealthChecks;

public class ElasticHealthCheck(IElasticStatusChecker statusChecker) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        var health = await statusChecker.GetHealthStatusAsync(cancellationToken);
        switch (health)
        {
            case Health.Green:
                return HealthCheckResult.Healthy();
            case Health.Yellow:
                return HealthCheckResult.Degraded();
            case Health.Red:
            default:
                return HealthCheckResult.Unhealthy();
        }
    }
}