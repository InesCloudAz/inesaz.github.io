using System.ComponentModel.DataAnnotations;

namespace Todo.Web.Models;

public sealed class TodoEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public bool IsCompleted { get; set; }
}
