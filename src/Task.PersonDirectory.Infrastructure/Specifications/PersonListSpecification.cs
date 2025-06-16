using Microsoft.EntityFrameworkCore;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Infrastructure.Specifications;

public class PersonListSpecification : Specification<Person>
{
    public PersonListSpecification(bool quickSearch, PersonListSpecificationArgs args)
    {
        if (quickSearch && !string.IsNullOrWhiteSpace(args.QuickSearch))
        {
            SetCriteria(person =>
                EF.Functions.Like(person.FirstName, $"%{args.QuickSearch}%") ||
                EF.Functions.Like(person.LastName, $"%{args.QuickSearch}%") ||
                EF.Functions.Like(person.PersonalNumber, $"%{args.QuickSearch}%")
            );
        }
        else
        {
            SetCriteria(person =>
                (string.IsNullOrWhiteSpace(args.FirstName) || person.FirstName == args.FirstName) &&
                (string.IsNullOrWhiteSpace(args.LastName) || person.LastName == args.LastName) &&
                (string.IsNullOrWhiteSpace(args.PersonalNumber) || person.PersonalNumber == args.PersonalNumber) &&
                (!args.Gender.HasValue || person.Gender == args.Gender) &&
                (!args.DateOfBirth.HasValue || person.DateOfBirth == args.DateOfBirth) &&
                (!args.CityId.HasValue || person.CityId == args.CityId)
            );
        }
    }

    public PersonListSpecification IncludePhoneNumbers()
    {
        AddInclude(a => a.PhoneNumbers);
        return this;
    }
    
    public PersonListSpecification IncludeRelatedPersons()
    {
        AddInclude(a => a.RelatedPersons);
        AddInclude("RelatedPersons.RelatedTo");
        return this;
    }
}

public record PersonListSpecificationArgs(
    string? QuickSearch = null,
    string? FirstName = null,
    string? LastName = null,
    string? PersonalNumber = null,
    Gender? Gender = null,
    DateTime? DateOfBirth = null,
    int? CityId = null
);