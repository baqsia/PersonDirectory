using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Domain.Aggregates;

public class RelatedPerson
{
    public int Id { get; init; }
    public RelatedPersonConnection ConnectionType { get; init; }

    public int PersonId { get; init; }
    public Person Person { get; init; } = null!;

    public int RelatedToId { get; init; }
    public Person RelatedTo { get; init; } = null!;

    public static RelatedPerson Connect(Person person, Person relatedPerson, RelatedPersonConnection connectionType)
    {
        return new RelatedPerson
        {
            PersonId = person.Id,
            Person = person,
            RelatedToId = relatedPerson.Id,
            RelatedTo = relatedPerson,
            ConnectionType = connectionType
        };
    }
}