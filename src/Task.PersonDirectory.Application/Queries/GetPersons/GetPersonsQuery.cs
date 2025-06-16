using Mediator;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Queries.GetPersons;

public record GetPersonsQuery(
    string? QuickSearch = null,
    string? FirstName = null,
    string? LastName = null,
    string? PersonalNumber = null,
    Gender? Gender = null,
    DateTime? DateOfBirth = null,
    int? CityId = null,
    int Page = 1,
    int PageSize = 10
): IRequest<ResponseResult<PagedResult<PersonDto>>>;