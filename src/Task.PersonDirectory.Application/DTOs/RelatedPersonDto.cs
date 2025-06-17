using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.DTOs;

public record RelatedPersonDto(
    int RelatedToId,
    RelatedPersonConnection Connection,
    string RelatedFirstName,
    string RelatedLastName
);