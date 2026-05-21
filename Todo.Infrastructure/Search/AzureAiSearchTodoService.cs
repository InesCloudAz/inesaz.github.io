using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Todo.Application.Abstractions;
using Todo.Application.Search;

namespace Todo.Infrastructure.Search;

public sealed class AzureAiSearchTodoService : IAzureAiSearchTodoIndexer, IAzureAiSearchTodoSearchService
{
    private readonly SearchClient? _searchClient;
    private readonly ILogger<AzureAiSearchTodoService> _logger;

    public AzureAiSearchTodoService(IOptions<AzureSearchOptions> options, ILogger<AzureAiSearchTodoService> logger)
    {
        _logger = logger;
        var searchOptions = options.Value;
        IsConfigured = searchOptions.IsConfigured;

        if (IsConfigured)
        {
            _searchClient = new SearchClient(
                new Uri(searchOptions.Endpoint!),
                searchOptions.IndexName!,
                new AzureKeyCredential(searchOptions.ApiKey!));
        }
    }

    public bool IsConfigured { get; }

    public async Task IndexTodoAsync(TodoSearchDocument todo, CancellationToken cancellationToken = default)
    {
        if (_searchClient is null)
        {
            return;
        }

        await _searchClient.MergeOrUploadDocumentsAsync([todo], cancellationToken: cancellationToken);
        _logger.LogInformation("Todo {TodoId} indexed in Azure AI Search", todo.Id);
    }

    public async Task RemoveTodoAsync(Guid todoId, string userId, CancellationToken cancellationToken = default)
    {
        if (_searchClient is null)
        {
            return;
        }

        await _searchClient.DeleteDocumentsAsync("Id", new[] { todoId.ToString() }, cancellationToken: cancellationToken);
        _logger.LogInformation("Todo {TodoId} removed from Azure AI Search for user {UserId}", todoId, userId);
    }

    public async Task<IReadOnlyList<TodoSearchDocument>> SearchTodosAsync(string searchTerm, string userId, CancellationToken cancellationToken = default)
    {
        if (_searchClient is null || string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var options = new SearchOptions
        {
            Filter = $"UserId eq '{EscapeODataString(userId)}'",
            Size = 50
        };

        var response = await _searchClient.SearchAsync<TodoSearchDocument>(searchTerm.Trim(), options, cancellationToken);
        var documents = new List<TodoSearchDocument>();

        await foreach (var result in response.Value.GetResultsAsync().WithCancellation(cancellationToken))
        {
            documents.Add(result.Document);
        }

        return documents;
    }

    private static string EscapeODataString(string value) => value.Replace("'", "''");
}
