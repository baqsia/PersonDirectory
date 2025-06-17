using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.DTOs;

public record AddRelationPersonDto(int PersonId, RelatedPersonConnection Connection);