using FluentValidation;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Application.Queries.GetPersons;

public class GetPersonsQueryValidation : AbstractValidator<GetPersonsQuery>
{
    public GetPersonsQueryValidation(IResourceLocalizer resourceLocalizer)
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage(resourceLocalizer.Localize(ResourceKeys.PageMustNotBeNegativeOrZero));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(resourceLocalizer.Localize(ResourceKeys.PageSizeMustBeBetween1to100));
    }
}