# DocuQuery

AI-powered document Q&A — upload a PDF, ask questions, get answers with citations.

**Live demo:** [docuquery.ezana.dev](https://docuquery.ezana.dev)

## What it does

DocuQuery lets you upload any PDF document and ask natural language questions about it. The app finds the most relevant passages using vector similarity search and uses GPT-4o to generate accurate answers with source citations.

## Architecture
<img width="1440" height="720" alt="image" src="https://github.com/user-attachments/assets/049a6ac5-fad1-4377-a7cc-d4f9f4f10b2a" />

Angular 21 → ASP.NET Core 10 API → Azure OpenAI (GPT-4o + text-embedding-3-large)
                                 → Azure AI Search (vector store)
                                 → Azure Blob Storage (raw PDFs)

**Ingest pipeline:** PDF upload → PdfPig text extraction → sliding window chunking → batched embedding generation → Azure AI Search upsert

**Query pipeline:** Question → embedding → vector similarity search (top 5 chunks) → Agent Framework passes context to GPT-4o → answer with citations

## Tech stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21, Angular Material, TypeScript |
| Backend | ASP.NET Core 10 Web API, C# |
| AI orchestration | Microsoft Agent Framework 1.0 |
| Vector store | Azure AI Search |
| Embeddings + chat | Azure OpenAI (text-embedding-3-large + GPT-4o) |
| Document storage | Azure Blob Storage |
| CI/CD | GitHub Actions → Azure App Service + Azure Static Web Apps |

## Local setup

### Prerequisites
- .NET 10 SDK
- Node.js 22+ and Angular CLI 21
- Azure subscription with Azure OpenAI, AI Search, and Storage resources

### Backend

```bash
cd DocuQuery.Api
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR_RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key"
dotnet user-secrets set "AzureOpenAI:ChatDeployment" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:EmbeddingDeployment" "text-embedding-3-large"
dotnet user-secrets set "AzureAISearch:Endpoint" "https://YOUR_SEARCH.search.windows.net"
dotnet user-secrets set "AzureAISearch:ApiKey" "your-key"
dotnet user-secrets set "AzureStorage:ConnectionString" "your-connection-string"
dotnet run
```

### Frontend

```bash
cd docuquery-web
npm install
ng serve
```

## Author

**Ezana Tefera** — [ezana.dev](https://ezana.dev) · [LinkedIn](https://linkedin.com/in/ezanatefera)
