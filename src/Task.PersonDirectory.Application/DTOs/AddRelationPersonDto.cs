using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Application.DTOs;

public record AddRelationPersonDto(int PersonId, RelatedPersonConnection Connection);