using Moq;
using Shouldly;
using Task.PersonDirectory.Application.Commands.CreatePerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Commands.CreatePerson;

[TestFixture]
public class CreatePersonCommandHandlerTests
{
    private Mock<IPersonRepository> _personRepositoryMock = null!;
    private Mock<ICityRepository> _cityRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IOutboxDispatcher> _dispatcherMock = null!;
    private CreatePersonCommandHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _cityRepositoryMock = new Mock<ICityRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dispatcherMock = new Mock<IOutboxDispatcher>();

        _sut = new CreatePersonCommandHandler(
            _personRepositoryMock.Object,
            _cityRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dispatcherMock.Object
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturnCityNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        var command = GetValidCommand();
        _cityRepositoryMock.Setup(x => x.GetBySpecificationAsync(
                It.IsAny<GetCityByIdSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            id => id.ShouldBeOfType<CityNotFound>(),
            a => a.ShouldBe(0)
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldAddPerson_AndSaveChanges_AndDispatchEvent_WhenDataIsValid()
    {
        // Arrange
        var command = GetValidCommand();
        var fakeCity = new City
        {
            Name = "Tbilisi",
            Id = 1
        };

        _cityRepositoryMock
            .Setup(x => x.GetBySpecificationAsync(It.IsAny<GetCityByIdSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeCity);

        Person? addedPerson = null;

        _personRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .Callback<Person, CancellationToken>((p, _) => addedPerson = p)
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _dispatcherMock.Setup(x => x.DispatchAsync(It.IsAny<PersonCreated>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            id => id.ShouldBeNull(),
            a => a.ShouldBe(addedPerson!.Id)
        );

        _personRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherMock.Verify(x => x.DispatchAsync(It.IsAny<PersonCreated>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Application.Commands.CreatePerson.CreatePersonCommand GetValidCommand() =>
        new(
            FirstName: "John",
            LastName: "Doe",
            Gender: Gender.Male,
            PersonalNumber: "123456789",
            DateOfBirth: new DateTime(1990, 1, 1),
            CityId: 1,
            PhoneNumbers: [new PhoneNumberDto(MobileType.Home, "555123456")]
        );
}