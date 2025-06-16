using System.Text;
using FluentValidation.TestHelper;
using Moq;
using Task.PersonDirectory.Application.Commands.UploadPersonImage;
using Task.PersonDirectory.Application.Common;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Services;
using Task.PersonDirectory.UnitTests.Fixtures;

namespace Task.PersonDirectory.UnitTests.Commands.UploadPersonImage;

[TestFixture]
public class UploadPersonImageCommandValidatorTests
{
    private Mock<IResourceLocalizer> _resourceLocalizer = null!;
    private UploadPersonImageCommandValidator _sut = null!;

    [SetUp]
    public void Setup()
    {
        _resourceLocalizer = new Mock<IResourceLocalizer>();
        _resourceLocalizer.Setup(rl => rl.Localize(It.IsAny<string>()))
            .Returns<string>(key => key);
        _sut = new UploadPersonImageCommandValidator(_resourceLocalizer.Object);
    }

    private static async Task<UploadPersonImageCommand> CreateValidCommand()
    {
        var file = new TestFormFile(new MemoryStream(Encoding.UTF8.GetBytes(new string('a', 1024))), "avatar.jpg",
            1024);

        await using var stream = file.OpenReadStream();
        var fileDto = new FileUploadDto(
            Content: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            Length: file.Length
        );
        return new UploadPersonImageCommand(1, fileDto);
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Pass_When_Command_Is_Valid()
    {
        var command = await CreateValidCommand();
        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async System.Threading.Tasks.Task Should_Fail_When_PersonId_Is_Invalid()
    {
        var command = await CreateValidCommand() with { PersonId = 0 };
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.PersonId);
    }

    [Test]
    public void Should_Fail_When_File_Is_Null()
    {
        var command = new UploadPersonImageCommand(1, null!);
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.File);
    }

    [Test]
    public void Should_Fail_When_File_Is_Empty()
    {
        var emptyStream = new MemoryStream();
        var fileDto = new FileUploadDto(
            Content: emptyStream,
            FileName: "avatar.jpg",
            ContentType: "nothing",
            Length: 0
        );
        var command = new UploadPersonImageCommand(1, fileDto);
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor("File.Length");
    }

    [Test]
    public void Should_Fail_When_File_Too_Large()
    {
        var largeStream = new MemoryStream(Encoding.UTF8.GetBytes(new string('a', 3 * 1024 * 1024)));
        var fileDto = new FileUploadDto(
            Content: largeStream,
            FileName: "avatar.jpg",
            ContentType: "nothing",
            Length: 3 * 1024 * 1024
        );
        var command = new UploadPersonImageCommand(1, fileDto);
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor("File.Length");
    }

    [TestCase(".exe")]
    [TestCase(".txt")]
    [TestCase(".bmp")]
    public void Should_Fail_When_Invalid_File_Extension(string extension)
    {
        var stream = new MemoryStream("dummy"u8.ToArray());
        var fileDto = new FileUploadDto(
            Content: stream,
            FileName: $"avatar{extension}",
            ContentType: "nothing",
            Length: 100
        );
        var command = new UploadPersonImageCommand(1, fileDto);
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor("File");
    }

    [TestCase(".jpg")]
    [TestCase(".jpeg")]
    [TestCase(".png")]
    public void Should_Pass_For_Valid_File_Extensions(string extension)
    {
        var stream = new MemoryStream("dummy"u8.ToArray());
        var fileDto = new FileUploadDto(
            Content: stream,
            FileName: $"avatar{extension}",
            ContentType: "nothing",
            Length: 100
        );
        var command = new UploadPersonImageCommand(1, fileDto);
        var result = _sut.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("File.FileName");
    }
}