using Microsoft.Extensions.Logging;
using Moq;

namespace Task.PersonDirectory.UnitTests.Fixtures;

public static class LoggerExtensions
{
    public static void VerifyLog<TInstance>(
        this Mock<ILogger<TInstance>> logger,
        LogLevel logLevel,
        string expectedMessage,
        Times times
    )
    {
        logger.Verify(l => l.Log(
            logLevel,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString() != null && v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), times);
    }
}