using FluentValidation.TestHelper;
using Moq;
using Task.PersonDirectory.Application.Commands.CreatePerson;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain.ValueObjects;

namespace Task.PersonDirectory.UnitTests.Commands.CreatePerson;

[TestFixture]
public class CreatePersonCommandValidatorTests
{
    private Mock<IResourceLocalizer> _resourceLocalizer = null!;
    private CreatePersonCommandValidator _sut = null!;

    [SetUp]
    public void Setup()
    {
        _resourceLocalizer = new Mock<IResourceLocalizer>();
        _resourceLocalizer.Setup(rl => rl.Localize(It.IsAny<string>()))
            .Returns<string>(key => key);
        _sut = new CreatePersonCommandValidator(_resourceLocalizer.Object);
    }

    [Test]
    public void Should_Have_Error_When_FirstName_Is_Empty()
    {
        var model = GetValidCommand() with { FirstName = "" };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Should_Have_Error_When_Latin_And_Georgian_Mixed()
    {
        var model = GetValidCommand() with { FirstName = "ჯონJohn" };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Test]
    public void Should_Not_Have_Error_When_Only_Latin()
    {
        var model = GetValidCommand() with { FirstName = "John", LastName = "Smith" };
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Test]
    public void Should_Have_Error_When_Underage()
    {
        var model = GetValidCommand() with { DateOfBirth = DateTime.Today.AddYears(-10) };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }
 
    [Test]
    public void Should_Have_Error_When_Invalid_PersonalNumber()
    {
        var model = GetValidCommand() with { PersonalNumber = "abc" };
        var result = _sut.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PersonalNumber);
    }
 
    [Test]
    public void Should_Pass_For_Valid_Command()
    {
        var model = GetValidCommand();
        var result = _sut.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreatePersonCommand GetValidCommand() =>
        new(
            FirstName: "John",
            LastName: "Smith",
            Gender: Gender.Male,
            PersonalNumber: "12345678901",
            DateOfBirth: DateTime.Today.AddYears(-20),
            CityId: 1,
            PhoneNumbers: [new PhoneNumberDto(MobileType.Home, "555123456")]
        );
}