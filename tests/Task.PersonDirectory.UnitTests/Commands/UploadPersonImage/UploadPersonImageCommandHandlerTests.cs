using Moq;
using Shouldly;
using Task.PersonDirectory.Application.Commands.UploadPersonImage;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Events;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Commands.UploadPersonImage;

public class UploadPersonImageCommandHandlerTests
{
    private Mock<IPersonRepository> _personRepoMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<IImageStorage> _imageStorageMock = null!;
    private Mock<IOutboxDispatcher> _dispatcherMock = null!;
    private UploadPersonImageCommandHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _personRepoMock = new Mock<IPersonRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _imageStorageMock = new Mock<IImageStorage>();
        _dispatcherMock = new Mock<IOutboxDispatcher>();

        _sut = new UploadPersonImageCommandHandler(
            _personRepoMock.Object,
            _unitOfWorkMock.Object,
            _imageStorageMock.Object,
            _dispatcherMock.Object
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturnPersonNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        var command = new UploadPersonImageCommand(99,
            new FileUploadDto(new MemoryStream(), "test", "application/png", 0));
        _personRepoMock.Setup(r =>
                r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Switch(
            notFound => notFound.ShouldBeOfType<PersonNotFound>(),
            res => res.Result.ShouldBeFalse()
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldUploadImageAndDispatchEvent_WhenPersonExists()
    {
        // Arrange
        var person = Person.Create("John", "Doe", Gender.Male, "12345678901", DateTime.Today.AddYears(-30), 1);
        var command = new UploadPersonImageCommand(person.Id,
            new FileUploadDto(new MemoryStream(), "test", "application/png", 0));

        _personRepoMock.Setup(r =>
                r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        _imageStorageMock.Setup(s =>
                s.SaveAsync(person.Id, command.File, It.IsAny<CancellationToken>()))
            .ReturnsAsync("images/john_doe.png");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert

        result.Switch(
            notFound => notFound.ShouldBeNull(),
            res => res.Result.ShouldBeTrue()
        );

        _personRepoMock.Verify(r => r.Update(It.Is<Person>(p => p.ImagePath == "images/john_doe.png")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _dispatcherMock.Verify(d => d.DispatchAsync(
                It.Is<PersonUpdated>(e =>
                    e.Id == person.Id &&
                    e.ImagerUrl == "images/john_doe.png"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}