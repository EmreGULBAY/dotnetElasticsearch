using SampleApi.Models;

namespace SampleApi.Interfaces
{
    public interface IDocumentService<Document>
    {
        Task<Document> GetDocumentAsync(int id);
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<string> AddDocumentAsync(Document document);
        Task UpdateDocumentAsync(Document document);
        Task DeleteDocumentAsync(int id);
    }
}
