using System.Text;
using Shouldly;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Services;

namespace Task.PersonDirectory.UnitTests.Services;

[TestFixture]
public class FileSystemImageStorageTests
{
    private string _basePath = null!;
    private FileSystemImageStorage _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_basePath);
        _sut = new FileSystemImageStorage(_basePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }

    [Test]
    public async System.Threading.Tasks.Task SaveAsync_SavesFileAndReturnsRelativePath()
    {
        // Arrange
        var personId = 123;
        var content = "fake image data";
        var fileName = "avatar.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var file = new FileUploadDto(stream, fileName, "application/image", stream.Length);

        // Act
        var relativePath = await _sut.SaveAsync(personId, file, CancellationToken.None);

        // Assert
        relativePath.ShouldBe(Path.Combine("images", "person_123.jpg"));
        var savedFilePath = Path.Combine(_basePath, relativePath);
        File.Exists(savedFilePath).ShouldBeTrue();

        var savedContent = await File.ReadAllTextAsync(savedFilePath);
        savedContent.ShouldBe(content);
    }

    [Test]
    public async System.Threading.Tasks.Task LoadBase64Async_ReturnsBase64EncodedImage_WhenFileExists()
    {
        // Arrange
        var fileName = "person_456.png";
        var content = "image content";
        var relativePath = Path.Combine("images", fileName);
        var fullPath = Path.Combine(_basePath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, content);

        // Act
        var result = await _sut.LoadBase64Async(relativePath, CancellationToken.None);

        // Assert
        var expectedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        result.ShouldBe($"data:image/png;base64,{expectedBase64}");
    }

    [Test]
    public async System.Threading.Tasks.Task LoadBase64Async_ReturnsNull_WhenFileDoesNotExist()
    {
        var result = await _sut.LoadBase64Async("nonexistent.jpg", CancellationToken.None);
        
        result.ShouldBeNull();
    }

    [Test]
    public async System.Threading.Tasks.Task LoadBase64Async_ReturnsNull_WhenPathIsNullOrEmpty()
    {
        var result1 = await _sut.LoadBase64Async(null, CancellationToken.None);
        var result2 = await _sut.LoadBase64Async("", CancellationToken.None);
        
        // Assert
        result1.ShouldBeNull();
        result2.ShouldBeNull();
    }
}