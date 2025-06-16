namespace Task.PersonDirectory.Application.DTOs;

public record ResponseResult<TResult>(TResult Result)
{
    public static implicit operator ResponseResult<TResult>(TResult result) => new(result);
}