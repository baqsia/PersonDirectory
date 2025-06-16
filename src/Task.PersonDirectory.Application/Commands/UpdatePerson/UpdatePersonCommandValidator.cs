using FluentValidation;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Common.ValidationPipeline;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.Application.Commands.UpdatePerson;

public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{
    public UpdatePersonCommandValidator(IResourceLocalizer localizer)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(localizer.Localize(ResourceKeys.FirstNameIsRequired))
            .Length(2, 50).WithMessage(localizer.Localize(ResourceKeys.FirstNameRange))
            .Must(ValidationRegexUtil.BeOnlyGeorgianOrOnlyLatin)
            .WithMessage(localizer.Localize(ResourceKeys.FirstNameLettersValidation));

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(localizer.Localize(ResourceKeys.LastNameIsRequired))
            .Length(2, 50).WithMessage(localizer.Localize(ResourceKeys.LastNameRange))
            .Must(ValidationRegexUtil.BeOnlyGeorgianOrOnlyLatin)
            .WithMessage(localizer.Localize(ResourceKeys.LastNameLettersValidation));

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage(localizer.Localize(ResourceKeys.GenderMustBeMaleOrFemale));

        RuleFor(x => x.PersonalNumber)
            .NotEmpty().WithMessage(localizer.Localize(ResourceKeys.PersonalNumberIsRequired))
            .Length(11).WithMessage(localizer.Localize(ResourceKeys.PersonalNumberLength))
            .Matches("^[0-9]{11}$").WithMessage(localizer.Localize(ResourceKeys.PersonalNumberDigitsOnly));

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(localizer.Localize(ResourceKeys.DateOfBirthIsRequired))
            .Must(ValidationRegexUtil.BeAtLeast18)
            .WithMessage(localizer.Localize(ResourceKeys.MustBeAtLeast18));

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage(localizer.Localize(ResourceKeys.CityIdMustBePositive));

        RuleForEach(x => x.PhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.Type)
                    .IsInEnum().WithMessage(localizer.Localize(ResourceKeys.PhoneNumberTypeInvalid));

                phone.RuleFor(p => p.Number)
                    .NotEmpty().WithMessage(localizer.Localize(ResourceKeys.PhoneNumberIsRequired))
                    .Length(4, 50).WithMessage(localizer.Localize(ResourceKeys.PhoneNumberLengthRange));
            });
    }
}