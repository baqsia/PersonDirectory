using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using Shouldly;
using Task.PersonDirectory.Application.Common.SyncPerson;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Queries.GetConnectionReport;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Domain.ValueObjects;
using Task.PersonDirectory.Infrastructure.Context;
using Task.PersonDirectory.Infrastructure.Repositories;

namespace Task.PersonDirectory.UnitTests.Queries;

[TestFixture]
public class GetConnectionReportQueryHandlerTests
{
    private Mock<IElasticClient> _elasticClientMock = null!;
    private Mock<IElasticStatusChecker> _elasticStatusCheckerMock = null!;
    private Mock<IRelatedPersonRepository> _relatedPersonRepoMock = null!;
    private Mock<ILogger<GetConnectionReportQueryHandler>> _loggerMock = null!;
    private GetConnectionReportQueryHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _elasticClientMock = new Mock<IElasticClient>();
        _elasticStatusCheckerMock = new Mock<IElasticStatusChecker>();
        _relatedPersonRepoMock = new Mock<IRelatedPersonRepository>();
        _loggerMock = new Mock<ILogger<GetConnectionReportQueryHandler>>();

        _sut = new GetConnectionReportQueryHandler(
            _elasticClientMock.Object,
            _elasticStatusCheckerMock.Object,
            _relatedPersonRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldUseElasticsearch_WhenHealthIsNotRed()
    {
        // Arrange
        var query = new GetConnectionReportQuery();
        const int personId = 1;

        _elasticStatusCheckerMock
            .Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Health.Green);

        var searchResponseMock = new Mock<ISearchResponse<PersonSearchDocument>>();
        searchResponseMock.Setup(x => x.IsValid).Returns(true);
        searchResponseMock.Setup(x => x.Documents).Returns(new List<PersonSearchDocument>
        {
            new()
            {
                PersonId = personId,
                Relations =
                [
                    new RelatedPersonDto(1, RelatedPersonConnection.Acquaintance, "test1", "test1 lastname"),
                    new RelatedPersonDto(2, RelatedPersonConnection.Colleague, "test2", "test2 lastname"),
                    new RelatedPersonDto(3, RelatedPersonConnection.Acquaintance, "test3", "test3 lastname")
                ]
            }
        });

        _elasticClientMock
            .Setup(x => x.SearchAsync(
                It.IsAny<Func<SearchDescriptor<PersonSearchDocument>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponseMock.Object);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Result.Count.ShouldBe(1);
        result.Result.First().PersonId.ShouldBe(personId);
        result.Result.First().Counts[RelatedPersonConnection.Acquaintance].ShouldBe(2);
        result.Result.First().Counts[RelatedPersonConnection.Colleague].ShouldBe(1);
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldUseDatabase_WhenElasticHealthIsRed()
    {
        // Arrange
        var query = new GetConnectionReportQuery();
        const int personId = 1;

        _elasticStatusCheckerMock
            .Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Health.Red);

        _relatedPersonRepoMock
            .Setup(x => x.GetGroupedByConnectionTypeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                (personId, RelatedPersonConnection.Acquaintance, 2),
                (personId, RelatedPersonConnection.Colleague, 1)
            ]);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Result.Count.ShouldBe(1);
        result.Result.First().PersonId.ShouldBe(personId);
        result.Result.First().Counts[RelatedPersonConnection.Acquaintance].ShouldBe(2);
        result.Result.First().Counts[RelatedPersonConnection.Colleague].ShouldBe(1);
    }

    [Test]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenElasticFails()
    {
        // Arrange
        var query = new GetConnectionReportQuery();
        _elasticStatusCheckerMock
            .Setup(x => x.GetHealthStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Health.Green);

        var searchResponseMock = new Mock<ISearchResponse<PersonSearchDocument>>();
        searchResponseMock.Setup(x => x.IsValid).Returns(false);
        searchResponseMock.Setup(x => x.OriginalException).Returns(new Exception("ES failed"));

        _elasticClientMock
            .Setup(x => x.SearchAsync<PersonSearchDocument>(
                It.IsAny<Func<SearchDescriptor<PersonSearchDocument>, ISearchRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResponseMock.Object);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Result.ShouldBeEmpty();
    }
}