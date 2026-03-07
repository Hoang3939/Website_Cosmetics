using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Services;

namespace Website_Cosmetics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IRAGService _ragService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IRAGService ragService, ILogger<ChatController> logger)
        {
            _ragService = ragService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message is required" });
                }

                var sessionId = request.SessionId ?? HttpContext.Session.Id ?? Guid.NewGuid().ToString();
                var response = await _ragService.ProcessMessageAsync(request.Message, sessionId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("generate-embeddings")]
        [Authorize] // Only authenticated users (add admin check if needed)
        public async Task<IActionResult> GenerateEmbeddings([FromQuery] int? productId = null)
        {
            try
            {
                if (productId.HasValue)
                {
                    var success = await _ragService.GenerateEmbeddingForProductAsync(productId.Value);
                    if (success)
                    {
                        return Ok(new { message = $"Embedding generated for product {productId}" });
                    }
                    return BadRequest(new { error = "Failed to generate embedding" });
                }
                else
                {
                    var success = await _ragService.GenerateEmbeddingsForAllProductsAsync();
                    if (success)
                    {
                        return Ok(new { message = "Embeddings generated for all products" });
                    }
                    return BadRequest(new { error = "Failed to generate embeddings" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embeddings");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
    }
}


