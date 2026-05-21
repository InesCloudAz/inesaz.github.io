using Microsoft.Extensions.Logging;
using Moq;
using Todo.Application.Abstractions;
using Todo.Application.DTOs;
using Todo.Application.Services;
using Todo.Domain.Entities;

namespace Todo.Tests;

public class TodoServiceTests
{
    [Fact]
    public async Task CreateTodoAsync_AddsTodoForCurrentUser()
    {
        var repository = new InMemoryTodoRepository();
        var service = CreateService(repository);

        var todo = await service.CreateTodoAsync(new CreateTodoRequest
        {
            Title = "Build Azure pipeline",
            Description = "Restore, build, test, publish and deploy"
        }, "user-1");

        Assert.Equal("Build Azure pipeline", todo.Title);
        Assert.Single(await service.GetTodosAsync("user-1"));
    }

    [Fact]
    public async Task UpdateTodoAsync_ChangesTextAndStatus()
    {
        var repository = new InMemoryTodoRepository();
        var service = CreateService(repository);
        var created = await service.CreateTodoAsync(new CreateTodoRequest { Title = "Draft README" }, "user-1");

        var updated = await service.UpdateTodoAsync(created.Id, new UpdateTodoRequest
        {
            Title = "Polish README",
            Description = "Add architecture and Azure sections",
            IsCompleted = true
        }, "user-1");

        Assert.NotNull(updated);
        Assert.Equal("Polish README", updated.Title);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task GetTodosAsync_FiltersByStatus()
    {
        var repository = new InMemoryTodoRepository();
        var service = CreateService(repository);
        var completed = await service.CreateTodoAsync(new CreateTodoRequest { Title = "Done task" }, "user-1");
        await service.CreateTodoAsync(new CreateTodoRequest { Title = "Active task" }, "user-1");
        await service.MarkCompletedAsync(completed.Id, "user-1");

        var active = await service.GetTodosAsync("user-1", TodoStatusFilter.Active);
        var done = await service.GetTodosAsync("user-1", TodoStatusFilter.Completed);

        Assert.Single(active);
        Assert.Single(done);
        Assert.Equal("Active task", active[0].Title);
        Assert.Equal("Done task", done[0].Title);
    }

    [Fact]
    public async Task UserCanOnlyAccessOwnTodos()
    {
        var repository = new InMemoryTodoRepository();
        var service = CreateService(repository);
        var userOneTodo = await service.CreateTodoAsync(new CreateTodoRequest { Title = "Private task" }, "user-1");

        var result = await service.GetTodoAsync(userOneTodo.Id, "user-2");
        var updateResult = await service.UpdateTodoAsync(userOneTodo.Id, new UpdateTodoRequest { Title = "Hijack", IsCompleted = true }, "user-2");

        Assert.Null(result);
        Assert.Null(updateResult);
        Assert.Single(await service.GetTodosAsync("user-1"));
        Assert.Empty(await service.GetTodosAsync("user-2"));
    }

    private static TodoService CreateService(ITodoRepository repository)
    {
        var logger = new Mock<ILogger<TodoService>>();
        return new TodoService(repository, logger.Object);
    }

    private sealed class InMemoryTodoRepository : ITodoRepository
    {
        private readonly List<TodoItem> _todos = [];

        public Task<IReadOnlyList<TodoItem>> GetForUserAsync(string userId, TodoStatusFilter status, CancellationToken cancellationToken = default)
        {
            IEnumerable<TodoItem> query = _todos.Where(todo => todo.UserId == userId);
            query = status switch
            {
                TodoStatusFilter.Active => query.Where(todo => !todo.IsCompleted),
                TodoStatusFilter.Completed => query.Where(todo => todo.IsCompleted),
                _ => query
            };

            return Task.FromResult<IReadOnlyList<TodoItem>>(query.ToList());
        }

        public Task<TodoItem?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_todos.FirstOrDefault(todo => todo.Id == id && todo.UserId == userId));

        public Task AddAsync(TodoItem todo, CancellationToken cancellationToken = default)
        {
            _todos.Add(todo);
            return Task.CompletedTask;
        }

        public void Remove(TodoItem todo) => _todos.Remove(todo);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
