using Nest;
using SampleApi.Interfaces;
using SampleApi.Models;
using Microsoft.Extensions.Logging;

public class DocumentService : IDocumentService<Document>
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IElasticClient elasticClient, ILogger<DocumentService> logger)
    {
        _elasticClient = elasticClient;
        _logger = logger;

        EnsureIndexExists();
    }

    private void EnsureIndexExists()
    {
        var existsResponse = _elasticClient.Indices.Exists("documents");
        if (!existsResponse.Exists)
        {
            var createIndexResponse = _elasticClient.Indices.Create("documents", c => c
                .Map<Document>(m => m
                    .AutoMap()
                )
            );

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError("Failed to create documents index: {0}", createIndexResponse.OriginalException.Message);
            }
        }
    }

    public async Task<Document> GetDocumentAsync(int id)
    {
        _logger.LogInformation($"Getting document with ID: {id}");
        var response = await _elasticClient.GetAsync<Document>(id);
        if (response.IsValid)
        {
            _logger.LogInformation($"Got document: {response.Source}");
            return response.Source;
        }
        else
        {
            _logger.LogError($"Failed to get document with ID: {id}, Error: {response.ServerError?.Error?.Reason}");
            return null;
        }
    }

    public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
    {
        _logger.LogInformation("Getting all documents");
        var response = await _elasticClient.SearchAsync<Document>(s => s
            .Query(q => q.MatchAll()));

        if (response.IsValid)
        {
            _logger.LogInformation($"Got {response.Documents.Count} documents");
        }
        else
        {
            _logger.LogError("Failed to get documents", response.OriginalException);
        }

        return response.Documents;
    }

    public async Task<string> AddDocumentAsync(Document document)
    {
        _logger.LogInformation($"Adding document with ID: {document.Id}");
        var response = await _elasticClient.IndexDocumentAsync(document);
        if (response.IsValid)
        {
            _logger.LogInformation("Document added successfully");
            return "success";
        }
        else
        {
            _logger.LogError($"Failed to add document, Error: {response.ServerError?.Error?.Reason}");
            return "false";
        }
    }

    public async Task UpdateDocumentAsync(Document document)
    {
        _logger.LogInformation($"Updating document with ID: {document.Id}");
        var response = await _elasticClient.UpdateAsync<Document>(document.Id, u => u
            .Doc(document));

        if (response.IsValid)
        {
            _logger.LogInformation("Document updated successfully");
        }
        else
        {
            _logger.LogError($"Failed to update document with ID: {document.Id}, Error: {response.ServerError?.Error?.Reason}");
        }
    }

    public async Task DeleteDocumentAsync(int id)
    {
        _logger.LogInformation($"Deleting document with ID: {id}");
        var response = await _elasticClient.DeleteAsync<Document>(id);

        if (response.IsValid)
        {
            _logger.LogInformation("Document deleted successfully");
        }
        else
        {
            _logger.LogError($"Failed to delete document with ID: {id}, Error: {response.ServerError?.Error?.Reason}");
        }
    }
}
