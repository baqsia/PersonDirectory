using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Application.DTOs;

public record RelatedPersonTypeCountDto(
    int PersonId,
    Dictionary<RelatedPersonConnection, int> Counts
);
