using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions;
using Todo.Application.DTOs;
using Todo.Domain.Entities;
using Todo.Infrastructure.Persistence;

namespace Todo.Infrastructure.Repositories;

public sealed class TodoRepository : ITodoRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TodoRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TodoItem>> GetForUserAsync(string userId, TodoStatusFilter status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Todos
            .Where(todo => todo.UserId == userId);

        query = status switch
        {
            TodoStatusFilter.Active => query.Where(todo => !todo.IsCompleted),
            TodoStatusFilter.Completed => query.Where(todo => todo.IsCompleted),
            _ => query
        };

        return await query
            .OrderBy(todo => todo.IsCompleted)
            .ThenByDescending(todo => todo.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> SearchForUserAsync(string userId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedTerm = searchTerm.Trim();

        return await _dbContext.Todos
            .Where(todo => todo.UserId == userId)
            .Where(todo =>
                todo.Title.Contains(normalizedTerm) ||
                (todo.Description != null && todo.Description.Contains(normalizedTerm)))
            .OrderBy(todo => todo.IsCompleted)
            .ThenByDescending(todo => todo.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<TodoItem?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken = default) =>
        _dbContext.Todos.FirstOrDefaultAsync(todo => todo.Id == id && todo.UserId == userId, cancellationToken);

    public async Task AddAsync(TodoItem todo, CancellationToken cancellationToken = default) =>
        await _dbContext.Todos.AddAsync(todo, cancellationToken);

    public void Remove(TodoItem todo) => _dbContext.Todos.Remove(todo);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
