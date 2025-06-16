namespace Task.PersonDirectory.Application.Errors;

public sealed record RelationNotFound(int PersonId, int RelatedToId);