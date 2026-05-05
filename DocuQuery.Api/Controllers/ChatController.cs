using DocuQuery.Api.Models;
using DocuQuery.Api.Services;
using Microsoft.AspNetCore.Mvc;

using ChatResponseDto = DocuQuery.Api.Models.ChatResult;

namespace DocuQuery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(RagAgentService ragAgent, ILogger<ChatController> logger) : ControllerBase
    {
        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponseDto>> Ask(
            [FromBody] ChatRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question cannot be empty.");

            try
            {
                var response = await ragAgent.AskAsync(request, ct);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Chat failed for question: {Question}", request.Question);
                return StatusCode(500, "Failed to process question.");
            }
        }
    }
}