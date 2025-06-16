using Mediator;
using OneOf;
using Task.PersonDirectory.Application.DTOs;
using Task.PersonDirectory.Application.Errors;

namespace Task.PersonDirectory.Application.Commands.UploadPersonImage;

public record UploadPersonImageCommand(
    int PersonId,
    FileUploadDto File
) : IRequest<OneOf<PersonNotFound, ResponseResult<bool>>>;