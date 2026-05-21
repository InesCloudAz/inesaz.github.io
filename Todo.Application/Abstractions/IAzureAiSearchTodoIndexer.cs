using Todo.Application.DTOs;

namespace Todo.Application.Abstractions;

public interface IAzureAiSearchTodoIndexer
{
    Task IndexTodoAsync(TodoDto todo, CancellationToken cancellationToken = default);
    Task RemoveTodoAsync(Guid todoId, string userId, CancellationToken cancellationToken = default);
}
