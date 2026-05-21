using System.ComponentModel.DataAnnotations;

namespace Todo.Application.DTOs;

public sealed class CreateTodoRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime? DueDateUtc { get; set; }
}
