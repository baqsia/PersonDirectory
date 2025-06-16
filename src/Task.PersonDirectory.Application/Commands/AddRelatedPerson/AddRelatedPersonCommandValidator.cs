using FluentValidation;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Application.Commands.AddRelatedPerson;

public class AddRelatedPersonCommandValidator : AbstractValidator<AddRelatedPersonCommand>
{
    public AddRelatedPersonCommandValidator(IResourceLocalizer resourceLocalizer)
    {
        RuleFor(x => x.PersonId)
            .GreaterThan(0)
            .WithMessage(resourceLocalizer.Localize(ResourceKeys.PersonIdMustBeGreaterThanZero));

        RuleFor(x => x.Relate.PersonId)
            .GreaterThan(0)
            .WithMessage(resourceLocalizer.Localize(ResourceKeys.RelatedPersonIdMustBeGreaterThanZero));

        RuleFor(x => x)
            .Must(x => x.Relate.PersonId != x.PersonId)
            .WithMessage(resourceLocalizer.Localize(ResourceKeys.PersonCantBeRelatedToThemselves));
    }
}