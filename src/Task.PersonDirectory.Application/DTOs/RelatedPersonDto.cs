using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Application.DTOs;

public record RelatedPersonDto(
    int RelatedToId,
    RelatedPersonConnection Connection,
    string RelatedFirstName,
    string RelatedLastName
);