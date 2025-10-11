using MediatR;

namespace Core.Application.Commands;

public record IndexDocumentCommand<T> : IRequest where T : class
{
    public T Document { get; init; } = default!;
    public string Index { get; init; } = string.Empty;
    public string? Id { get; init; }
}
