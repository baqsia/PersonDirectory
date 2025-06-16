namespace Task.PersonDirectory.Application.DTOs;

public record FileUploadDto(
    Stream Content,
    string FileName,
    string ContentType,
    long Length
);