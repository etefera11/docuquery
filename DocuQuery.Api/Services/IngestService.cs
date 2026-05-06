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
        public async Task<IngestResponse> IngestAsync(IFormFile file, string sessionId, CancellationToken ct = default)
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

            // 3. Extract all chunks first
            var chunks = new List<DocumentChunk>();
            using var pdf = PdfDocument.Open(file.OpenReadStream());
            foreach (var page in pdf.GetPages())
            {
                var content = string.Join(" ", page.GetWords().Select(w => w.Text));
                if (string.IsNullOrWhiteSpace(content)) continue;

                chunks.Add(new DocumentChunk
                {
                    FileName = file.FileName,
                    PageNumber = page.Number,
                    Content = content,
                    SessionId = sessionId
                });
            }

            // 4. Embed in batches of 20
            const int batchSize = 20;
            for (int i = 0; i < chunks.Count; i += batchSize)
            {
                var batch = chunks.Skip(i).Take(batchSize).ToList();
                var texts = batch.Select(c => c.Content).ToList();

                var embeddings = await embeddingGenerator.GenerateAsync(texts, cancellationToken: ct);

                for (int j = 0; j < batch.Count; j++)
                    batch[j].Embedding = embeddings[j].Vector;

                var upsertTasks = batch.Select(async chunk => await collection.UpsertAsync(chunk, ct));
                await Task.WhenAll(upsertTasks);
                chunksCreated += batch.Count;
            }

            logger.LogInformation("Ingested {FileName} with {ChunksCreated} chunks", file.FileName, chunksCreated);
            return new IngestResponse(file.FileName, chunksCreated);
        }
    }
}