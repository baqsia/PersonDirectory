using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Domain;

public class RelatedPerson
{
    public int Id { get; set; }
    public RelatedPersonConnection ConnectionType { get; set; } = default!; // colleague, acquaintance, relative, other

    public int PersonId { get; set; }
    public Person Person { get; set; } = default!;

    public int RelatedToId { get; set; }
    public Person RelatedTo { get; set; } = default!;

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