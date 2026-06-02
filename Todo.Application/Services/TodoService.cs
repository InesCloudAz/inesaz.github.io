using Microsoft.Extensions.Logging;
using Todo.Application.Abstractions;
using Todo.Application.DTOs;
using Todo.Application.Search;
using Todo.Domain.Entities;

namespace Todo.Application.Services;

public sealed class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly IAzureAiSearchTodoIndexer _searchIndexer;
    private readonly IAzureAiSearchTodoSearchService _searchService;
    private readonly ILogger<TodoService> _logger;

    public TodoService(
        ITodoRepository repository,
        IAzureAiSearchTodoIndexer searchIndexer,
        IAzureAiSearchTodoSearchService searchService,
        ILogger<TodoService> logger)
    {
        _repository = repository;
        _searchIndexer = searchIndexer;
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TodoDto>> GetTodosAsync(string userId, TodoStatusFilter status = TodoStatusFilter.All, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todos = await _repository.GetForUserAsync(userId, status, cancellationToken);
        return todos.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<TodoDto>> SearchTodosAsync(string userId, string searchTerm, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetTodosAsync(userId, TodoStatusFilter.All, cancellationToken);
        }

        if (_searchService.IsConfigured)
        {
            try
            {
                var searchResults = await _searchService.SearchTodosAsync(searchTerm, userId, cancellationToken);
                return searchResults.Select(MapSearchDocument).ToList();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Azure AI Search failed. Falling back to database search for user {UserId}", userId);
            }
        }

        var todos = await _repository.SearchForUserAsync(userId, searchTerm, cancellationToken);
        return todos.Select(Map).ToList();
    }

    public async Task<TodoDto?> GetTodoAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todo = await _repository.GetByIdForUserAsync(id, userId, cancellationToken);
        return todo is null ? null : Map(todo);
    }

    public async Task<TodoDto> CreateTodoAsync(CreateTodoRequest request, string userId, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todo = new TodoItem(request.Title, request.Description, request.DueDateUtc, userId);

        await _repository.AddAsync(todo, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Todo {TodoId} created for user {UserId}", todo.Id, userId);
        var dto = Map(todo);
        await TryIndexTodoAsync(todo, cancellationToken);
        return dto;
    }

    public async Task<TodoDto?> UpdateTodoAsync(Guid id, UpdateTodoRequest request, string userId, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todo = await _repository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (todo is null)
        {
            return null;
        }

        todo.Update(request.Title, request.Description, request.DueDateUtc);
        if (request.IsCompleted)
        {
            todo.MarkCompleted();
        }
        else
        {
            todo.MarkActive();
        }

        await _repository.SaveChangesAsync(cancellationToken);
        var dto = Map(todo);
        await TryIndexTodoAsync(todo, cancellationToken);
        return dto;
    }

    public async Task<bool> MarkCompletedAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todo = await _repository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (todo is null)
        {
            return false;
        }

        todo.MarkCompleted();
        await _repository.SaveChangesAsync(cancellationToken);
        await TryIndexTodoAsync(todo, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteTodoAsync(Guid id, string userId, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todo = await _repository.GetByIdForUserAsync(id, userId, cancellationToken);
        if (todo is null)
        {
            return false;
        }

        _repository.Remove(todo);
        await _repository.SaveChangesAsync(cancellationToken);
        await TryRemoveFromIndexAsync(todo.Id, userId, cancellationToken);
        return true;
    }

    public async Task<TodoStatsDto> GetStatsAsync(string userId, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todos = await _repository.GetForUserAsync(userId, TodoStatusFilter.All, cancellationToken);
        var completed = todos.Count(todo => todo.IsCompleted);
        return new TodoStatsDto(todos.Count, completed, todos.Count - completed);
    }

    private static TodoDto Map(TodoItem todo) =>
        new(todo.Id, todo.Title, todo.Description, todo.IsCompleted, todo.CreatedAtUtc, todo.UpdatedAtUtc, todo.DueDateUtc, todo.CompletedAtUtc);

    private static TodoDto MapSearchDocument(TodoSearchDocument document) =>
        new(Guid.Parse(document.Id), document.Title, document.Description, document.IsCompleted, document.CreatedAt, document.UpdatedAt, null, null);

    private static TodoSearchDocument MapSearchDocument(TodoItem todo) =>
        new()
        {
            Id = todo.Id.ToString(),
            UserId = todo.UserId,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAtUtc,
            UpdatedAt = todo.UpdatedAtUtc
        };

    private async Task TryIndexTodoAsync(TodoItem todo, CancellationToken cancellationToken)
    {
        if (!_searchIndexer.IsConfigured)
        {
            return;
        }

        try
        {
            await _searchIndexer.IndexTodoAsync(MapSearchDocument(todo), cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Azure AI Search indexing failed for todo {TodoId}", todo.Id);
        }
    }

    private async Task TryRemoveFromIndexAsync(Guid todoId, string userId, CancellationToken cancellationToken)
    {
        if (!_searchIndexer.IsConfigured)
        {
            return;
        }

        try
        {
            await _searchIndexer.RemoveTodoAsync(todoId, userId, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Azure AI Search remove failed for todo {TodoId}", todoId);
        }
    }

    private static void EnsureUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("An authenticated user is required.");
        }
    }
}
