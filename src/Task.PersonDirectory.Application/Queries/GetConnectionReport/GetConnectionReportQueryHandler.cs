using Elasticsearch.Net;
using Mediator;
using Microsoft.Extensions.Logging;
using Nest;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Application.Queries.GetConnectionReport;

public class GetConnectionReportQueryHandler(
    IElasticClient elasticClient,
    IElasticStatusChecker elasticStatusChecker,
    IRelatedPersonRepository relatedPersonRepository,
    ILogger<GetConnectionReportQueryHandler> logger
) : IRequestHandler<GetConnectionReportQuery, ResponseResult<List<RelatedPersonTypeCountDto>>>
{
    public async ValueTask<ResponseResult<List<RelatedPersonTypeCountDto>>> Handle(GetConnectionReportQuery query,
        CancellationToken cancellationToken)
    {
        var health = await elasticStatusChecker.GetHealthStatusAsync(cancellationToken);
        if (health != Health.Red)
        {
            return await QueryReadDatabaseAsync(cancellationToken);
        }

        return await QueryDatabaseAsync(cancellationToken);
    }

    private async Task<ResponseResult<List<RelatedPersonTypeCountDto>>> QueryDatabaseAsync(
        CancellationToken cancellationToken)
    {
        var persons = await relatedPersonRepository.GetGroupedByConnectionTypeAsync(cancellationToken);
         
        var result = persons
            .GroupBy(x => x.PersonId)
            .Select(g => new RelatedPersonTypeCountDto(
                g.Key,
                g.ToDictionary(x => x.Connection, x => x.Count)
            ))
            .ToList();

        return result;
    }

    private async Task<ResponseResult<List<RelatedPersonTypeCountDto>>> QueryReadDatabaseAsync(
        CancellationToken cancellationToken)
    {
        var searchResponse = await elasticClient.SearchAsync<PersonSearchDocument>(s =>
                s.Index("persons")
                    .Source(src => src
                        .Includes(i => i.Fields(f => f.PersonId, f => f.Relations))
                    ),
            cancellationToken
        );

        if (!searchResponse.IsValid)
        {
            logger.LogError(searchResponse.OriginalException, searchResponse.OriginalException?.Message);
            return new ResponseResult<List<RelatedPersonTypeCountDto>>([]);
        }

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