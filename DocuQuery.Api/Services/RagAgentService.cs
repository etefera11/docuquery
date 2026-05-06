using DocuQuery.Api.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using System.ComponentModel;
using System.Text.Json;

namespace DocuQuery.Api.Services
{
    public class RagAgentService(
        IChatClient chatClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        AzureAISearchVectorStore vectorStore,
        ILogger<RagAgentService> logger)
    {
        public async Task<ChatResult> AskAsync(ChatRequest request, CancellationToken ct = default)
        {
            var searchTool = AIFunctionFactory.Create(
                async ([Description("The user's question")] string question) =>
                {
                    var embeddings = await embeddingGenerator.GenerateAsync(
                        [question], cancellationToken: ct);
                    var vector = embeddings[0].Vector;

                    var collection = vectorStore.GetCollection<string, DocumentChunk>("documents");
                    var searchResults = collection.SearchAsync(
                       vector,
                       top: 5,
                       options: new VectorSearchOptions<DocumentChunk>
                       {
                           Filter = r => r.SessionId == request.SessionId
                       },
                       cancellationToken: ct);

                    var chunks = new List<object>();
                    await foreach (var result in searchResults)
                    {
                        chunks.Add(new
                        {
                            fileName = result.Record.FileName,
                            pageNumber = result.Record.PageNumber,
                            content = result.Record.Content
                        });
                    }

                    return JsonSerializer.Serialize(chunks);
                },
                name: "search_documents",
                description: "Search uploaded documents for content relevant to the question");

            var agent = new ChatClientAgent(
                chatClient,
                name: "DocuQueryAgent",
                instructions: """
                    You are a document assistant. Use search_documents to find relevant content.
                    Always cite sources as (FileName, p.X) for every claim.
                    If nothing relevant is found, say so clearly.
                    """,
                tools: [searchTool]);

            var response = await agent.RunAsync(
                new ChatMessage(ChatRole.User, request.Question),
                cancellationToken: ct);

            logger.LogInformation("Agent answered: {Question}", request.Question);
            return new ChatResult(response.Text ?? string.Empty, []);
        }
    }
}