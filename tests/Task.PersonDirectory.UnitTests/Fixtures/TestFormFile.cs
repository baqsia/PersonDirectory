using Microsoft.AspNetCore.Http;

namespace Task.PersonDirectory.UnitTests.Fixtures;


public class TestFormFile(Stream stream, string fileName, long length) : IFormFile
{
    public Stream OpenReadStream() => stream;
    public void CopyTo(Stream target) => stream.CopyTo(target);
    public async System.Threading.Tasks.Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
        await stream.CopyToAsync(target, cancellationToken);

    public string ContentType { get; } = "image/jpeg";
    public string ContentDisposition { get; set; } = string.Empty;
    public IHeaderDictionary Headers { get; set; } = null!;
    public long Length { get; } = length;
    public string Name => "file";
    public string FileName { get; } = fileName;
}