using Microsoft.Extensions.Logging;
using Todo.Application.Abstractions;
using Todo.Application.DTOs;
using Todo.Domain.Entities;

namespace Todo.Application.Services;

public sealed class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;
    private readonly ILogger<TodoService> _logger;

    public TodoService(ITodoRepository repository, ILogger<TodoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TodoDto>> GetTodosAsync(string userId, TodoStatusFilter status = TodoStatusFilter.All, CancellationToken cancellationToken = default)
    {
        EnsureUser(userId);
        var todos = await _repository.GetForUserAsync(userId, status, cancellationToken);
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
        return Map(todo);
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
        return Map(todo);
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
        new(todo.Id, todo.Title, todo.Description, todo.IsCompleted, todo.CreatedAtUtc, todo.DueDateUtc, todo.CompletedAtUtc);

    private static void EnsureUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("An authenticated user is required.");
        }
    }
}
