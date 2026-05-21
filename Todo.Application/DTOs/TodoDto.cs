namespace Todo.Application.DTOs;

public sealed record TodoDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAtUtc,
    DateTime? DueDateUtc,
    DateTime? CompletedAtUtc);
