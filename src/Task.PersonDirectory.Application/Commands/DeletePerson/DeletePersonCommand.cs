using Mediator;
using OneOf;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;

namespace Task.PersonDirectory.Application.Commands.DeletePerson;

public record DeletePersonCommand(int PersonId): IRequest<OneOf<PersonNotFound, ResponseResult<bool>>>;