using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Application.Abstractions;
using Todo.Application.DTOs;
using Todo.Web.Models;

namespace Todo.Web.Controllers;

[Authorize]
public class TodosController : Controller
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public async Task<IActionResult> Index(TodoStatusFilter filter = TodoStatusFilter.All, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var model = new TodoDashboardViewModel
        {
            Todos = await _todoService.GetTodosAsync(userId, filter, cancellationToken),
            Stats = await _todoService.GetStatsAsync(userId, cancellationToken),
            CurrentFilter = filter
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoRequest newTodo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await Index(TodoStatusFilter.All, cancellationToken);
        }

        await _todoService.CreateTodoAsync(newTodo, GetUserId(), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var todo = await _todoService.GetTodoAsync(id, GetUserId(), cancellationToken);
        if (todo is null)
        {
            return NotFound();
        }

        return View(new TodoEditViewModel
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            DueDateUtc = todo.DueDateUtc,
            IsCompleted = todo.IsCompleted
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TodoEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new UpdateTodoRequest
        {
            Title = model.Title,
            Description = model.Description,
            DueDateUtc = model.DueDateUtc,
            IsCompleted = model.IsCompleted
        };

        var todo = await _todoService.UpdateTodoAsync(model.Id, request, GetUserId(), cancellationToken);
        return todo is null ? NotFound() : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        await _todoService.MarkCompletedAsync(id, GetUserId(), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _todoService.DeleteTodoAsync(id, GetUserId(), cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Missing authenticated user id.");
}
