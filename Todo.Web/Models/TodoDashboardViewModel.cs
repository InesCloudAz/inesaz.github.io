using Todo.Application.DTOs;

namespace Todo.Web.Models;

public sealed class TodoDashboardViewModel
{
    public IReadOnlyList<TodoDto> Todos { get; init; } = [];
    public TodoStatsDto Stats { get; init; } = new(0, 0, 0);
    public TodoStatusFilter CurrentFilter { get; init; }
    public CreateTodoRequest NewTodo { get; init; } = new();
}
