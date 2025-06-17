using Elasticsearch.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Nest;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Application.Queries.GetConnectionReport;

public class GetConnectionReportQueryHandler(
    IElasticClient elasticClient,
    PersonDirectoryContext context
) : IRequestHandler<GetConnectionReportQuery, ResponseResult<List<RelatedPersonTypeCountDto>>>
{
    public async ValueTask<ResponseResult<List<RelatedPersonTypeCountDto>>> Handle(GetConnectionReportQuery query,
        CancellationToken cancellationToken)
    {
        var health = await elasticClient.Cluster.HealthAsync(ct: cancellationToken);
        if (health.Status != Health.Red)
        {
            return await QueryReadDatabaseAsync(cancellationToken);
        }

        return await QueryDatabaseAsync(cancellationToken);
    }

    private async Task<ResponseResult<List<RelatedPersonTypeCountDto>>> QueryDatabaseAsync(CancellationToken cancellationToken)
    {
        var groupedData = await context.Set<RelatedPerson>()
            .GroupBy(rp => new { rp.PersonId, rp.ConnectionType })
            .Select(g => new
            {
                g.Key.PersonId,
                g.Key.ConnectionType,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var result = groupedData
            .GroupBy(x => x.PersonId)
            .Select(g => new RelatedPersonTypeCountDto(
                g.Key,
                g.ToDictionary(x => x.ConnectionType, x => x.Count)
            ))
            .ToList();

        return result;
    }

    private async Task<ResponseResult<List<RelatedPersonTypeCountDto>>> QueryReadDatabaseAsync(CancellationToken cancellationToken)
    {
        var searchResponse = await elasticClient.SearchAsync<PersonSearchDocument>(s =>
                s.Index("persons")
                    .Source(src => src
                        .Includes(i => i.Fields(f => f.PersonId, f => f.Relations))
                    ),
            cancellationToken
        );

        if (!searchResponse.IsValid)
            throw new Exception($"Elasticsearch error: {searchResponse.ServerError?.Error?.Reason}");

        var result = searchResponse.Documents
            .Where(a => a.Relations.Count != 0)
            .Select(p => new RelatedPersonTypeCountDto(
                p.PersonId,
                p.Relations
                    .GroupBy(rp => rp.Connection)
                    .ToDictionary(g => g.Key, g => g.Count())
            ))
            .ToList();

        return result;
    }
}