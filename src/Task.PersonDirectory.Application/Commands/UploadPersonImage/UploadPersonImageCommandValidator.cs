using FluentValidation;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.Application.Commands.UploadPersonImage;

public class UploadPersonImageCommandValidator : AbstractValidator<UploadPersonImageCommand>
{
    public UploadPersonImageCommandValidator(IResourceLocalizer resourceLocalizer)
    {
        RuleFor(x => x.PersonId)
            .GreaterThan(0).WithMessage(resourceLocalizer.Localize(ResourceKeys.PersonIdMustBeGreaterThanZero));

        RuleFor(x => x.File)
            .NotEmpty()
            .WithMessage(resourceLocalizer.Localize(ResourceKeys.FileIsRequired))
            .ChildRules(cr =>
            {
                cr.RuleFor(x => x.Length)
                    .GreaterThan(0)
                    .WithMessage(resourceLocalizer.Localize(ResourceKeys.FileIsEmpty))
                    .LessThanOrEqualTo(2 * 1024 * 1024)
                    .WithMessage(resourceLocalizer.Localize(ResourceKeys.ImageMinimumSize));

                cr.RuleFor(x => Path.GetExtension(x.FileName).ToLowerInvariant())
                    .Must(ext => ext is ".jpg" or ".jpeg" or ".png")
                    .WithMessage(
                        resourceLocalizer.Localize(ResourceKeys.ImageTypesAllowed));
            });
    }
}