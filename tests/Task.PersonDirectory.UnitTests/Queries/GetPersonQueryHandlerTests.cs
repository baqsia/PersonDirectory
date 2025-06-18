using Moq;
using Shouldly;
using Task.PersonDirectory.Application.Errors;
using Task.PersonDirectory.Application.Queries.GetPerson;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Queries;

[TestFixture]
public class GetPersonQueryHandlerTests
{
    private Mock<IPersonRepository> _personRepositoryMock = null!;
    private Mock<IImageStorage> _imageStorageMock = null!;
    private GetPersonByIdQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _imageStorageMock = new Mock<IImageStorage>();
        _sut = new GetPersonByIdQueryHandler(_personRepositoryMock.Object, _imageStorageMock.Object);
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturn_PersonNotFound_WhenPersonIsNull()
    {
        // Arrange
        var query = new GetPersonByIdQuery(10);
        _personRepositoryMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Switch(notFound =>
            {
                notFound.ShouldBeOfType<PersonNotFound>();
                notFound.PersonId.ShouldBe(10);
            },
            person => person.ShouldBeNull()
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturn_PersonDto_WhenPersonExists()
    {
        // Arrange
        var person = Person.Create("Baqar", "Gogia", Gender.Male, "12345678901", new DateTime(2000, 1, 1), 1)
            .WithNumbers([new PhoneNumber { Number = "1234", Type = MobileType.Home }]);

        var related = Person.Create("Anna", "Smith", Gender.Female, "09876543210", new DateTime(1990, 5, 10), 2);
        person.ApplyRelation(related, RelatedPersonConnection.Relative);
 
        person.ImagePath = "img.png";

        _personRepositoryMock
            .Setup(r => r.GetBySpecificationAsync(It.IsAny<GetPersonByIdSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(person);

        _imageStorageMock
            .Setup(s => s.LoadBase64Async("img.png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("base64-image");

        var query = new GetPersonByIdQuery(42);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert

        result.Switch(
            notFound => notFound.ShouldBeNull(),
            res =>
            {
                res.ShouldNotBeNull();
                var data = res.Result;
                data.Image.ShouldBe("base64-image");
                data.PhoneNumbers.Count.ShouldBe(1);
                data.RelatedPersons.Count.ShouldBe(1);
            }
        );
    }
}