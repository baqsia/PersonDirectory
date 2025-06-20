﻿using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Domain.Aggregates;

public class Person
{
    private Person()
    {
    }

    public int Id { get; private init; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Gender Gender { get; private set; }
    public string PersonalNumber { get; private set; } = null!;
    public DateTime DateOfBirth { get; private set; }
    public string? ImagePath { get; set; }
    public int CityId { get; private set; }
    public City City { get; private init; } = null!;

    private readonly List<PhoneNumber> _phoneNumbers = [];
    public IReadOnlyList<PhoneNumber> PhoneNumbers => _phoneNumbers.AsReadOnly();

    private readonly List<RelatedPerson> _relatedPersons = [];
    public IReadOnlyList<RelatedPerson> RelatedPersons => _relatedPersons.AsReadOnly();

    public static Person Create(
        string firstName,
        string lastName,
        Gender gender,
        string personalNumber,
        DateTime dateOfBirth,
        int cityId
    )
    {
        return new Person
        {
            FirstName = firstName,
            LastName = lastName,
            Gender = gender,
            PersonalNumber = personalNumber,
            DateOfBirth = dateOfBirth,
            CityId = cityId
        };
    }

    public Person WithNumbers(List<PhoneNumber> numbers)
    {
        _phoneNumbers.AddRange(numbers);
        return this;
    }

    public Person Modify(
        string firstName,
        string lastName,
        Gender gender,
        string personalNumber,
        DateTime dateOfBirth,
        int cityId
    )
    {
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        PersonalNumber = personalNumber;
        DateOfBirth = dateOfBirth;
        CityId = cityId;

        return this;
    }

    public bool ApplyRelation(Person relatedPerson, RelatedPersonConnection connection)
    {
        var existingRelation = RelatedPersons.FirstOrDefault(a => a.RelatedToId == relatedPerson.Id);
        if (existingRelation is not null)
        {
            _relatedPersons.Remove(existingRelation);
            return false;
        }

        _relatedPersons.Add(RelatedPerson.Connect(this, relatedPerson, connection));
        return true;
    }
}