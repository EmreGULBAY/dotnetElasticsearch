using Microsoft.AspNetCore.Mvc;
using SampleApi.Interfaces;
using SampleApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService<Document> _documentService;

    public DocumentsController(IDocumentService<Document> documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null)
        {
            return NotFound();
        }
        return Ok(document);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDocuments()
    {
        var documents = await _documentService.GetAllDocumentsAsync();
        return Ok(documents);
    }

    [HttpPost]
    public async Task<IActionResult> AddDocument([FromBody] Document document)
    {
        await _documentService.AddDocumentAsync(document);
        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] Document document)
    {
        if (id != document.Id)
        {
            return BadRequest();
        }
        await _documentService.UpdateDocumentAsync(document);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        await _documentService.DeleteDocumentAsync(id);
        return NoContent();
    }
}