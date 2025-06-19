using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Shouldly;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.Queries.GetPersons;
using Task.PersonDirectory.Application.Repository;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Repositories;
using Task.PersonDirectory.Infrastructure.Specifications;

namespace Task.PersonDirectory.UnitTests.Queries;

[TestFixture]
public class GetPersonsQueryHandlerTests
{
    private Mock<IElasticStatusChecker> _elasticStatusCheckerMock = null!;
    private Mock<IElasticClient> _elasticClientMock = null!;
    private Mock<IPersonRepository> _personRepoMock = null!;
    private Mock<ILogger<GetPersonsQueryHandler>> _loggerMock = null!;
    private Mock<IImageStorage> _imageStorageMock = null!;
    private GetPersonsQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GetPersonsQueryHandler>>();
        _elasticStatusCheckerMock = new Mock<IElasticStatusChecker>();
        _elasticClientMock = new Mock<IElasticClient>();
        _personRepoMock = new Mock<IPersonRepository>();
        _imageStorageMock = new Mock<IImageStorage>();

        _sut = new GetPersonsQueryHandler(
            _elasticStatusCheckerMock.Object,
            _loggerMock.Object,
            _elasticClientMock.Object,
            _personRepoMock.Object,
            _imageStorageMock.Object
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldUseElastic_WhenHealthIsNotRed()
    {
        // Arrange
        var query = new GetPersonsQuery { Page = 1, PageSize = 10 };
        _elasticStatusCheckerMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Health.Green);
 
        var personSearchDocs = new List<PersonSearchDocument>
        {
            new()
            {
                PersonId = 1,
                FirstName = "John",
                LastName = "Doe",
                Gender = Gender.Male,
                PersonalNumber = "12345678901",
                DateOfBirth = new DateTime(1990, 1, 1),
                CityId = 1,
                ImageUrl = "image/path.jpg",
                PhoneNumbers = [],
                Relations = []
            }
        };

        var mockResponse = new Mock<ISearchResponse<PersonSearchDocument>>();
        mockResponse.Setup(r => r.Documents).Returns(personSearchDocs);
        mockResponse.Setup(r => r.Total).Returns(1);
        mockResponse.Setup(r => r.IsValid).Returns(true);
        
        _elasticClientMock
            .Setup(ec => ec.SearchAsync(
                It.IsAny<Func<SearchDescriptor<PersonSearchDocument>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _imageStorageMock.Setup(i => i.LoadBase64Async(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("base64Image");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Result.Items.Count.ShouldBe(1);
        result.Result.TotalCount.ShouldBe(1);
        result.Result.Items[0].Image.ShouldBe("base64Image");
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldUseDb_WhenElasticHealthIsRed()
    {
        // Arrange
        var query = new GetPersonsQuery { Page = 1, PageSize = 10 };
        _elasticStatusCheckerMock.Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Health.Red);

        _personRepoMock
            .Setup(repo => repo.GetAllAsync(
                It.IsAny<PersonListSpecification>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([Person.Create("Nika", "Abramishvili", Gender.Male, "12345678901", new DateTime(1995, 5, 5), 2)]);

        _personRepoMock
            .Setup(repo => repo.CountAsync(It.IsAny<PersonListSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _imageStorageMock
            .Setup(x => x.LoadBase64Async(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("imageString");

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        
        result.Result.Items.Count.ShouldBe(1);
        result.Result.TotalCount.ShouldBe(1);
        result.Result.Items.First().FirstName.ShouldBe("Nika");
    }
}