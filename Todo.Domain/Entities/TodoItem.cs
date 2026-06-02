namespace Todo.Domain.Entities;

public class TodoItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? DueDateUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string UserId { get; private set; } = string.Empty;

    private TodoItem()
    {
    }

    public TodoItem(string title, string? description, DateTime? dueDateUtc, string userId)
    {
        Rename(title);
        Description = description;
        DueDateUtc = dueDateUtc;
        UserId = string.IsNullOrWhiteSpace(userId) ? throw new ArgumentException("User id is required.", nameof(userId)) : userId;
    }

    public void Update(string title, string? description, DateTime? dueDateUtc)
    {
        Rename(title);
        Description = description;
        DueDateUtc = dueDateUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
        CompletedAtUtc ??= DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkActive()
    {
        IsCompleted = false;
        CompletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title.Trim();
    }
}
