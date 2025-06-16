using Elasticsearch.Net;
using Nest;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.UnitTests.Services;

[TestFixture]
public class ElasticStatusCheckerTests
{
    private IElasticClient _elasticClientMock = null!;
    private ElasticStatusChecker _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var connection = new InMemoryConnection();
        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var settings = new ConnectionSettings(connectionPool, connection);
        _elasticClientMock = new ElasticClient(settings);
        _sut = new ElasticStatusChecker(_elasticClientMock);
    }

    [Test]
    public async System.Threading.Tasks.Task GetHealthStatusAsync_Should_Return_Expected_Health()
    {
        // Act
        var result = await _sut.GetHealthStatusAsync(CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(Health.Green));
    }
}