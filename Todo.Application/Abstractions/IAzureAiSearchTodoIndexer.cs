using Todo.Application.Search;

namespace Todo.Application.Abstractions;

public interface IAzureAiSearchTodoIndexer
{
    bool IsConfigured { get; }
    Task IndexTodoAsync(TodoSearchDocument todo, CancellationToken cancellationToken = default);
    Task RemoveTodoAsync(Guid todoId, string userId, CancellationToken cancellationToken = default);
}
