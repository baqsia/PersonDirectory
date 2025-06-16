using Task.PersonDirectory.Application.DTOs;

namespace Task.PersonDirectory.Application.Services;

public interface IImageStorage
{
    Task<string> SaveAsync(int personId, FileUploadDto file, CancellationToken cancellationToken);
    Task<string?> LoadBase64Async(string? imagePath, CancellationToken cancellationToken);
}

public class FileSystemImageStorage(string basePath) : IImageStorage
{
    public async Task<string> SaveAsync(int personId, FileUploadDto file, CancellationToken ct)
    {
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"person_{personId}{ext}";
        var relativePath = Path.Combine("images", fileName);
        var absolutePath = Path.Combine(basePath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.Content.CopyToAsync(stream, ct);

        return relativePath;
    }

    public async Task<string?> LoadBase64Async(string? relativePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        var fullPath = Path.Combine(basePath, relativePath);

        if (!File.Exists(fullPath))
            return null;

        byte[] bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        string contentType = Path.GetExtension(fullPath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return $"data:{contentType};base64,{Convert.ToBase64String(bytes)}";
    }
}