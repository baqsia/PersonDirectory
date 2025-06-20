﻿using Elasticsearch.Net;
using Mediator;
using Microsoft.Extensions.Logging;
using Nest;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.Application.Queries.GetPersons;

public class GetPersonsQueryHandler(
    IElasticStatusChecker elasticStatusChecker,
    ILogger<GetPersonsQueryHandler> logger,
    IElasticClient elasticClient,
    IPersonRepository personRepository,
    IImageStorage imageStorage
) : IRequestHandler<GetPersonsQuery, ResponseResult<PagedResult<PersonDto>>>
{
    public async ValueTask<ResponseResult<PagedResult<PersonDto>>> Handle(GetPersonsQuery request,
        CancellationToken cancellationToken)
    {
        var health = await elasticStatusChecker.GetHealthStatusAsync(cancellationToken);
        if (health != Health.Red)
        {
            return await QueryReadDatabaseAsync(request, cancellationToken);
        }

        return await QueryDatabaseAsync(request, cancellationToken);
    }

    private async Task<ResponseResult<PagedResult<PersonDto>>> QueryDatabaseAsync(
        GetPersonsQuery request,
        CancellationToken cancellationToken
    )
    {
        var specification = new PersonListSpecification(
                !string.IsNullOrWhiteSpace(request.QuickSearch),
                new PersonListSpecificationArgs(
                    request.QuickSearch,
                    request.FirstName,
                    request.LastName,
                    request.PersonalNumber,
                    request.Gender,
                    request.DateOfBirth,
                    request.CityId
                )
            ).IncludeRelatedPersons()
            .IncludePhoneNumbers();

        var persons = await personRepository.GetAllAsync(specification, cancellationToken);
        var totalCount = await personRepository.CountAsync(specification, cancellationToken);

        var results = new List<PersonDto>();
        foreach (var person in persons)
        {
            var imageBase64 = await imageStorage.LoadBase64Async(person.ImagePath, cancellationToken);

            var dto = new PersonDto(
                person.Id,
                person.FirstName,
                person.LastName,
                person.Gender,
                person.PersonalNumber,
                person.DateOfBirth,
                person.CityId,
                imageBase64,
                person.PhoneNumbers.Select(pn => pn.ToDto()).ToList(),
                person.RelatedPersons.Select(rp => rp.ToDto()).ToList()
            );

            results.Add(dto);
        }

        return new PagedResult<PersonDto>(results, totalCount, request.Page, request.PageSize);
    }

    private async Task<ResponseResult<PagedResult<PersonDto>>> QueryReadDatabaseAsync(
        GetPersonsQuery request,
        CancellationToken cancellationToken
    )
    {
        var must = GenerateQueryContainer(request);

        var searchResponse = await elasticClient.SearchAsync<PersonSearchDocument>(s => s
                .Index("persons_ngram")
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(q => q.Bool(b => b.Must(must.ToArray())))
                .TrackTotalHits(),
            cancellationToken
        );

        if (!searchResponse.IsValid)
        {
            logger.LogError(searchResponse.OriginalException, searchResponse.OriginalException?.Message);
            return new ResponseResult<PagedResult<PersonDto>>(new PagedResult<PersonDto>([], 0, request.Page, request.PageSize));
        }

        var results = new List<PersonDto>();
        foreach (var doc in searchResponse.Documents)
        {
            var personDto = new PersonDto(
                doc.PersonId,
                doc.FirstName,
                doc.LastName,
                doc.Gender,
                doc.PersonalNumber,
                doc.DateOfBirth,
                doc.CityId,
                await imageStorage.LoadBase64Async(doc.ImageUrl, cancellationToken),
                doc.PhoneNumbers,
                doc.Relations
            );
            results.Add(personDto);
        }

        return new PagedResult<PersonDto>(results, (int)searchResponse.Total, request.Page, request.PageSize);
    }

    private static List<QueryContainer> GenerateQueryContainer(GetPersonsQuery request)
    {
        var must = new List<QueryContainer>();

        if (!string.IsNullOrWhiteSpace(request.QuickSearch))
        {
            must.Add(new BoolQuery
            {
                Should = new List<QueryContainer>
                {
                    new MatchQuery { Field = "firstName", Query = request.QuickSearch.ToLower() },
                    new MatchQuery { Field = "lastName", Query = request.QuickSearch.ToLower() },
                    new MatchQuery { Field = "personalNumber", Query = request.QuickSearch }
                },
                MinimumShouldMatch = 1
            });
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            must.Add(new TermQuery { Field = "firstName.keyword", Value = request.FirstName });

        if (!string.IsNullOrWhiteSpace(request.LastName))
            must.Add(new TermQuery { Field = "lastName.keyword", Value = request.LastName });

        if (!string.IsNullOrWhiteSpace(request.PersonalNumber))
            must.Add(new TermQuery { Field = "personalNumber.keyword", Value = request.PersonalNumber });

        if (request.Gender is not null)
            must.Add(new TermQuery { Field = "gender", Value = request.Gender });

        if (request.DateOfBirth is not null)
            must.Add(new TermQuery { Field = "dateOfBirth", Value = request.DateOfBirth.Value.Date });

        if (request.CityId is not null)
            must.Add(new TermQuery { Field = "cityId", Value = request.CityId.Value });

        return must;
    }
}