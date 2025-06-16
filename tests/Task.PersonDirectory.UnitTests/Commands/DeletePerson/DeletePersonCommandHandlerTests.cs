using Moq;
using Shouldly;
using Task.PersonDirectory.Application.Commands.DeletePerson;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Commands.DeletePerson;

[TestFixture]
public class DeletePersonCommandHandlerTests
{
    private Mock<IPersonRepository> _personRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IOutboxDispatcher> _dispatcherMock = null!;
    private DeletePersonCommandHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dispatcherMock = new Mock<IOutboxDispatcher>();

        _sut = new DeletePersonCommandHandler(
            _personRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dispatcherMock.Object
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturnPersonNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        var command = new DeletePersonCommand(123);
        _personRepositoryMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(res =>
            {
                res.ShouldBeOfType<PersonNotFound>();
                res.PersonId.ShouldBe(123);
            },
            res => res.Result.ShouldBeFalse()
        );

        _personRepositoryMock.Verify(r => r.Remove(It.IsAny<Person>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<PersonDeleted>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldRemovePersonAndReturnTrue_WhenPersonExists()
    {
        // Arrange
        var person = Person.Create("John", "Doe", Gender.Male, "12345678901", DateTime.Today.AddYears(-30), 1);
        var command = new DeletePersonCommand(person.Id);

        _personRepositoryMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        _personRepositoryMock
            .Setup(r => r.Remove(person));

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _dispatcherMock
            .Setup(d => d.DispatchAsync(It.IsAny<PersonDeleted>(), It.IsAny<CancellationToken>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            res => res.ShouldBeNull(),
            res => res.Result.ShouldBeTrue()
        );
        
        _personRepositoryMock.Verify(r => r.Remove(person), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherMock.Verify(
            d => d.DispatchAsync(It.Is<PersonDeleted>(e => e.Id == person.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}