using Moq;
using Shouldly;
using Task.PersonDirectory.Application.Commands.AddRelatedPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Repository.Specifications;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Commands.AddRelatedPerson;

[TestFixture]
public class AddRelatedPersonCommandHandlerTests
{
    private Mock<IPersonRepository> _personRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IOutboxDispatcher> _dispatcherMock = null!;
    private AddRelatedPersonCommandHandler _sut = null!;
    private Person _person = null!;
    private Person _relatedPerson = null!;

    [SetUp]
    public void SetUp()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dispatcherMock = new Mock<IOutboxDispatcher>();

        _sut = new AddRelatedPersonCommandHandler(
            _personRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dispatcherMock.Object
        );

        _person = Person.Create("John", "Doe", Gender.Male, "12345678901", new DateTime(1990, 1, 1), 1);
        _relatedPerson = Person.Create("Jane", "Smith", Gender.Female, "10987654321", new DateTime(1990, 1, 1), 2);
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Return_PersonNotFound_When_Person_Does_Not_Exist()
    {
        var command = new AddRelatedPersonCommand(_person.Id, new AddRelationPersonDto(_relatedPerson.Id, RelatedPersonConnection.Acquaintance));

        _personRepositoryMock.Setup(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Person>>(), CancellationToken.None))
            .ReturnsAsync((Person?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Switch(
            notFound => notFound.ShouldBeOfType<PersonNotFound>(),
            id => id.ShouldBeFalse()
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Return_PersonNotFound_When_RelatedPerson_Does_Not_Exist()
    {
        var command =
            new AddRelatedPersonCommand(_person.Id,  new AddRelationPersonDto(_relatedPerson.Id, RelatedPersonConnection.Acquaintance));

        _personRepositoryMock.SetupSequence(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Person>>(), CancellationToken.None))
            .ReturnsAsync(_person)
            .ReturnsAsync((Person?)null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Switch(
            notFound => notFound.ShouldBeOfType<PersonNotFound>(),
            id => id.ShouldBeFalse()
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Add_Relation_When_Valid()
    {
        var command =
            new AddRelatedPersonCommand(_person.Id, new AddRelationPersonDto(_relatedPerson.Id, RelatedPersonConnection.Acquaintance));

        _personRepositoryMock.SetupSequence(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Person>>(), CancellationToken.None))
            .ReturnsAsync(_person)
            .ReturnsAsync(_relatedPerson);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Switch(
            notFound => notFound.ShouldBeOfType<PersonNotFound>(),
            id => id.ShouldBeTrue()
        );
        
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
        _dispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<PersonUpdated>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Remove_Relation_When_Already_Exists()
    {
        var command =
            new AddRelatedPersonCommand(_person.Id,new AddRelationPersonDto(_relatedPerson.Id, RelatedPersonConnection.Colleague));

        _person.ApplyRelation(_relatedPerson, RelatedPersonConnection.Colleague);

        _personRepositoryMock.SetupSequence(r => r.GetBySpecificationAsync(It.IsAny<ISpecification<Person>>(), CancellationToken.None))
            .ReturnsAsync(_person)
            .ReturnsAsync(_relatedPerson);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Switch(
            notFound => notFound.ShouldBeNull(),
            id => id.ShouldBeFalse()
        );
    }
}