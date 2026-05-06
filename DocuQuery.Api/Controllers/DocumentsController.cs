using DocuQuery.Api.Models;
using DocuQuery.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocuQuery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController(IngestService ingestService, ILogger<DocumentsController> logger) : ControllerBase
    {
        private static readonly string[] AllowedExtensions = [".pdf"];
        private const long MaxFileSizeBytes = 20 * 1024 * 1024;

        [HttpPost("upload")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<ActionResult<IngestResponse>> Upload(IFormFile file, [FromForm] string sessionId, CancellationToken ct)
            {
                if (file is null || file.Length == 0)
                    return BadRequest("No file provided.");

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                    return BadRequest($"Only PDF files are supported.");

                if (file.Length > MaxFileSizeBytes)
                    return BadRequest("File exceeds the 20 MB limit.");

                try
                {
                    var result = await ingestService.IngestAsync(file, sessionId, ct);
                    logger.LogInformation("Ingested {FileName}: {Chunks} chunks", result.FileName, result.ChunksCreated);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ingest failed for {FileName}", file.FileName);
                    return StatusCode(500, "Failed to process document.");
                }
            }
    }
}