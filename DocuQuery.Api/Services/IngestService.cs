using Azure.Storage.Blobs;
using DocuQuery.Api.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using UglyToad.PdfPig;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

namespace DocuQuery.Api.Services
{
    public class IngestService(
        BlobServiceClient blobServiceClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        AzureAISearchVectorStore vectorStore,
        ILogger<IngestService> logger)
    {
        public async Task<IngestResponse> IngestAsync(IFormFile file, CancellationToken ct = default)
        {
            var chunksCreated = 0;

            // 1. Upload raw PDF to Blob Storage
            var containerClient = blobServiceClient.GetBlobContainerClient("documents");
            await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);
            var blobClient = containerClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
            }

            // 2. Get or create the vector collection
            var collection = vectorStore.GetCollection<string, DocumentChunk>("documents");
            await collection.EnsureCollectionExistsAsync(ct);

            // 3. Extract text with PdfPig, embed, and upsert
            using var pdf = PdfDocument.Open(file.OpenReadStream());
            foreach (var page in pdf.GetPages())
            {
                var content = string.Join(" ", page.GetWords().Select(w => w.Text));
                if (string.IsNullOrWhiteSpace(content)) continue;

                var embeddings = await embeddingGenerator.GenerateAsync(new[] { content }, cancellationToken: ct);
                var embedding = embeddings[0].Vector;

                var chunk = new DocumentChunk
                {
                    FileName = file.FileName,
                    PageNumber = page.Number,
                    Content = content,
                    Embedding = embedding
                };

                await collection.UpsertAsync(chunk, ct);
                chunksCreated++;
            }

            logger.LogInformation("Ingested {FileName} with {ChunksCreated} chunks", file.FileName, chunksCreated);
            return new IngestResponse(file.FileName, chunksCreated);
        }
    }
}