namespace Task.PersonDirectory.Application.Common.ValidationPipeline;

public delegate Task<TResponse> MessageHandlerDelegate<TResponse>();
