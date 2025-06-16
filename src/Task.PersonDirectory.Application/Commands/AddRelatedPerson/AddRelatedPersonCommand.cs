using Mediator;
using OneOf;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;

namespace Task.PersonDirectory.Application.Commands.AddRelatedPerson;

public record AddRelatedPersonCommand(
    int PersonId,
    AddRelationPersonDto  Relate
) : IRequest<OneOf<PersonNotFound, bool>>;