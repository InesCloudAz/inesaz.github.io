using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Application.Abstractions;
using Todo.Application.DTOs;

namespace Todo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoDto>>> GetTodos([FromQuery] TodoStatusFilter status = TodoStatusFilter.All, CancellationToken cancellationToken = default)
    {
        var todos = await _todoService.GetTodosAsync(GetUserId(), status, cancellationToken);
        return Ok(todos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TodoDto>> GetTodo(Guid id, CancellationToken cancellationToken)
    {
        var todo = await _todoService.GetTodoAsync(id, GetUserId(), cancellationToken);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoDto>> CreateTodo(CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = await _todoService.CreateTodoAsync(request, GetUserId(), cancellationToken);
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TodoDto>> UpdateTodo(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = await _todoService.UpdateTodoAsync(id, request, GetUserId(), cancellationToken);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> MarkCompleted(Guid id, CancellationToken cancellationToken)
    {
        var updated = await _todoService.MarkCompletedAsync(id, GetUserId(), cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTodo(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _todoService.DeleteTodoAsync(id, GetUserId(), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Missing authenticated user id.");
}
