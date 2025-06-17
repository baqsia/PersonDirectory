using FluentValidation.TestHelper;
using Moq;
using Task.PersonDirectory.Application.Commands.AddRelatedPerson;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.UnitTests.Commands.AddRelatedPerson;

[TestFixture]
public class AddRelatedPersonCommandValidatorTests
{
    private Mock<IResourceLocalizer> _resourceLocalizer = null!;
    private AddRelatedPersonCommandValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _resourceLocalizer = new Mock<IResourceLocalizer>();
        _resourceLocalizer.Setup(rl => rl.Localize(It.IsAny<string>()))
            .Returns<string>(key => key);
        _sut = new AddRelatedPersonCommandValidator(_resourceLocalizer.Object);
    }

    [Test]
    public void Should_Have_Error_When_PersonId_Is_Zero()
    {
        //Arrange
        var command = new AddRelatedPersonCommand(0, new AddRelationPersonDto(2, RelatedPersonConnection.Colleague));
        
        //Act
        var result = _sut.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor(c => c.PersonId)
            .WithErrorMessage(ResourceKeys.PersonIdMustBeGreaterThanZero);
    }

    [Test]
    public void Should_Have_Error_When_Person_Is_Related_To_Themselves()
    {
        //Arrange
        var id = 5;
        var command = new AddRelatedPersonCommand(id, new AddRelationPersonDto(id, RelatedPersonConnection.Colleague));

        //Act
        var result = _sut.TestValidate(command);

        //Assert
        result.ShouldHaveValidationErrorFor("")
            .WithErrorMessage(ResourceKeys.PersonCantBeRelatedToThemselves);
    }

    [Test]
    public void Should_Not_Have_Error_When_Valid()
    {
        //Arrange
        var command = new AddRelatedPersonCommand(1, new AddRelationPersonDto(2, RelatedPersonConnection.Colleague));

        //Act
        var result = _sut.TestValidate(command);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}