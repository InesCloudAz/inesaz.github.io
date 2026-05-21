using Todo.Application.DTOs;
using Todo.Domain.Entities;

namespace Todo.Application.Abstractions;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetForUserAsync(string userId, TodoStatusFilter status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TodoItem>> SearchForUserAsync(string userId, string searchTerm, CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task AddAsync(TodoItem todo, CancellationToken cancellationToken = default);
    void Remove(TodoItem todo);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
