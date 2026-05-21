using Todo.Application.DTOs;

namespace Todo.Application.Abstractions;

public interface ITodoService
{
    Task<IReadOnlyList<TodoDto>> GetTodosAsync(string userId, TodoStatusFilter status = TodoStatusFilter.All, CancellationToken cancellationToken = default);
    Task<TodoDto?> GetTodoAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateTodoAsync(CreateTodoRequest request, string userId, CancellationToken cancellationToken = default);
    Task<TodoDto?> UpdateTodoAsync(Guid id, UpdateTodoRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkCompletedAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTodoAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<TodoStatsDto> GetStatsAsync(string userId, CancellationToken cancellationToken = default);
}
