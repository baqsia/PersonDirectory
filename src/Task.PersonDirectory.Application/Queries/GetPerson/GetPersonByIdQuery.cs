using Mediator;
using OneOf;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;

namespace Task.PersonDirectory.Application.Queries.GetPerson;

public record GetPersonByIdQuery(int PersonId): IRequest<OneOf<PersonNotFound, ResponseResult<PersonDto>>>;