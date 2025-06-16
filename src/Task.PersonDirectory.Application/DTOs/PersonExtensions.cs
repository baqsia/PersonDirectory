using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Application.DTOs;

public static class PersonExtensions
{
    public static RelatedPersonDto ToDto(this RelatedPerson relatedPerson)
    {
        return new RelatedPersonDto(
            relatedPerson.RelatedToId,
            relatedPerson.ConnectionType,
            relatedPerson.RelatedTo.FirstName,
            relatedPerson.RelatedTo.LastName
        );
    }
    
    public static PhoneNumber ToPhoneNumber(this PhoneNumberDto phoneNumberDto)
    {
        return new PhoneNumber
        {
            Type = phoneNumberDto.Type,
            Number = phoneNumberDto.Number
        };
    }
    
    public static PhoneNumberDto ToDto(this PhoneNumber phoneNumberDto)
    {
        return new PhoneNumberDto(phoneNumberDto.Type, phoneNumberDto.Number);
    }
}