using Todo.Application.Search;

namespace Todo.Application.Abstractions;

public interface IAzureAiSearchTodoSearchService
{
    bool IsConfigured { get; }
    Task<IReadOnlyList<TodoSearchDocument>> SearchTodosAsync(string searchTerm, string userId, CancellationToken cancellationToken = default);
}
