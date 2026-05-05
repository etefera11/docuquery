using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using DocuQuery.Api.Services;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

var builder = WebApplication.CreateBuilder(args);

// Azure OpenAI client
var openAiClient = new AzureOpenAIClient(
    new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
    new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!));

// IChatClient
builder.Services.AddSingleton<IChatClient>(
    new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!))
    .GetChatClient(builder.Configuration["AzureOpenAI:ChatDeployment"]!)
    .AsIChatClient());

// Embedding generator
builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
    new AzureOpenAIClient(
        new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
        new AzureKeyCredential(builder.Configuration["AzureOpenAI:ApiKey"]!))
    .GetEmbeddingClient(builder.Configuration["AzureOpenAI:EmbeddingDeployment"]!)
    .AsIEmbeddingGenerator());

// Azure AI Search vector store
builder.Services.AddSingleton(new AzureAISearchVectorStore(
    new SearchIndexClient(
        new Uri(builder.Configuration["AzureAISearch:Endpoint"]!),
        new AzureKeyCredential(builder.Configuration["AzureAISearch:ApiKey"]!))));

// Azure Blob Storage
builder.Services.AddSingleton(
    new BlobServiceClient(builder.Configuration["AzureStorage:ConnectionString"]!));

// App services
builder.Services.AddScoped<IngestService>();
builder.Services.AddScoped<RagAgentService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://happy-bay-077ce800f7.azurestaticapps.net",
            "https://docuquery.ezana.dev")
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Angular");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();