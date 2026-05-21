namespace Todo.Infrastructure.Search;

public sealed class AzureSearchOptions
{
    public string? Endpoint { get; set; }
    public string? IndexName { get; set; }
    public string? ApiKey { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(IndexName) &&
        !string.IsNullOrWhiteSpace(ApiKey);
}
