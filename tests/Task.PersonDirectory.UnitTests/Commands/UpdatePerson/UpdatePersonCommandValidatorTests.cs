using FluentValidation.TestHelper;
using Moq;
using Task.PersonDirectory.Application.Commands.UpdatePerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.UnitTests.Commands.UpdatePerson;

[TestFixture]
public class UpdatePersonCommandValidatorTests
{
    private Mock<IResourceLocalizer> _resourceLocalizer = null!;
    private UpdatePersonCommandValidator _sut = null!;

    [SetUp]
    public void Setup()
    {
        _resourceLocalizer = new Mock<IResourceLocalizer>();
        _resourceLocalizer.Setup(rl => rl.Localize(It.IsAny<string>()))
            .Returns<string>(key => key);
        _sut = new UpdatePersonCommandValidator(_resourceLocalizer.Object);
    }

    private static UpdatePersonCommand GetValidCommand()
    {
        return new UpdatePersonCommand(
            PersonId: 1,
            FirstName: "John",
            LastName: "Doe",
            Gender: Gender.Male,
            PersonalNumber: "12345678901",
            DateOfBirth: DateTime.Today.AddYears(-20),
            CityId: 1,
            PhoneNumbers: [new PhoneNumberDto(MobileType.Home, "599123456")]
        );
    }

    [Test]
    public void ValidCommand_ShouldNotHaveValidationErrors()
    {
        var result = _sut.TestValidate(GetValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase("")]
    [TestCase("J")]
    [TestCase("დJ")]
    public void InvalidFirstName_ShouldHaveValidationError(string name)
    {
        var command = GetValidCommand() with { FirstName = name };
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [TestCase("")]
    [TestCase("D")]
    [TestCase("დD")]
    public void InvalidLastName_ShouldHaveValidationError(string name)
    {
        var command = GetValidCommand() with { LastName = name };
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
 
    [TestCase("123")]
    [TestCase("123456789012")]
    [TestCase("abcdefg1234")]
    public void InvalidPersonalNumber_ShouldHaveValidationError(string pn)
    {
        var command = GetValidCommand() with { PersonalNumber = pn };
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PersonalNumber);
    }

    [Test]
    public void Underage_ShouldHaveValidationError()
    {
        var command = GetValidCommand() with { DateOfBirth = DateTime.Today.AddYears(-17) };
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Test]
    public void InvalidCityId_ShouldHaveValidationError()
    {
        var command = GetValidCommand() with { CityId = 0 };
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CityId);
    }
}