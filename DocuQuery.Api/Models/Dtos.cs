namespace DocuQuery.Api.Models
{
    public record IngestResponse(string FileName, int ChunksCreated);

    public record ChatRequest(string Question, List<ChatTurn>? History = null);

    public record ChatTurn(string Role, string Content);

    public record ChatResult(string Answer, List<Citation> Citations);

    public record Citation(string FileName, int PageNumber, string Excerpt);
}