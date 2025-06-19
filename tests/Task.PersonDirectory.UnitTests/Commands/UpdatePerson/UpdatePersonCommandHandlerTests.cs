using Moq;
using Shouldly;
using Task.PersonDirectory.Application.Commands.UpdatePerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Commands.UpdatePerson;

[TestFixture]
public class UpdatePersonCommandHandlerTests
{
    private Mock<IPersonRepository> _personRepoMock = null!;
    private Mock<ICityRepository> _cityRepoMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IOutboxDispatcher> _dispatcherMock = null!;
    private UpdatePersonCommandHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _personRepoMock = new Mock<IPersonRepository>();
        _cityRepoMock = new Mock<ICityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dispatcherMock = new Mock<IOutboxDispatcher>();

        _sut = new UpdatePersonCommandHandler(
            _personRepoMock.Object,
            _cityRepoMock.Object,
            _unitOfWorkMock.Object,
            _dispatcherMock.Object
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturnPersonNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        var command = GetValidCommand();

        _personRepoMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            notFound => notFound.ShouldBeOfType<PersonNotFound>(),
            cityNotFound => cityNotFound.ShouldBeNull(),
            res => res.Result.ShouldBeFalse()
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturnCityNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        var command = GetValidCommand();
        var person = CreateSamplePerson();

        _personRepoMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        _cityRepoMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetCityByIdSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            notFound => notFound.ShouldBeNull(),
            cityNotFound => cityNotFound.ShouldBeOfType<CityNotFound>(),
            res => res.Result.ShouldBeFalse()
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldUpdatePerson_WhenPersonAndCityExist()
    {
        // Arrange
        var command = GetValidCommand();
        var person = CreateSamplePerson();
        var city = new City
        {
            Id = 1,
            Name = "Kutaisi"
        };

        _personRepoMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        _cityRepoMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetCityByIdSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _dispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<PersonUpdated>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            notFound => notFound.ShouldBeNull(),
            cityNotFound => cityNotFound.ShouldBeNull(),
            res => res.Result.ShouldBeTrue()
        );

        _personRepoMock.Verify(r => r.Update(It.IsAny<Person>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<PersonUpdated>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static UpdatePersonCommand GetValidCommand()
    {
        return new UpdatePersonCommand(
            PersonId: 1,
            FirstName: "John",
            LastName: "Doe",
            Gender: Gender.Male,
            PersonalNumber: "12345678901",
            DateOfBirth: DateTime.Today.AddYears(-25),
            CityId: 1,
            PhoneNumbers: [new PhoneNumberDto(MobileType.Home, "599123456")]
        );
    }

    private static Person CreateSamplePerson()
    {
        var person = Person.Create("Old", "Name", Gender.Male, "12345678901", DateTime.Today.AddYears(-30), 1);
        return person.WithNumbers([new PhoneNumber { Number = "1234", Type = MobileType.Home }]);
    }
}