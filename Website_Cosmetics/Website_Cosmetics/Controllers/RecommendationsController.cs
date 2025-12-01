using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Cosmetics.Services;

namespace Website_Cosmetics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationsController> _logger;

        public RecommendationsController(
            IRecommendationService recommendationService,
            ILogger<RecommendationsController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetRecommendations(int productId, [FromQuery] int topK = 5)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new { error = "Invalid product ID" });
                }

                if (topK < 1 || topK > 20)
                {
                    topK = 5; // Default to 5 if invalid
                }

                var recommendations = await _recommendationService.GetRecommendationsAsync(productId, topK);
                
                return Ok(new
                {
                    productId = productId,
                    recommendations = recommendations,
                    count = recommendations.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting recommendations for product {productId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("build-matrix")]
        [Authorize] // Only for admin
        public async Task<IActionResult> BuildSimilarityMatrix()
        {
            try
            {
                var matrix = await _recommendationService.BuildSimilarityMatrixAsync();
                return Ok(new
                {
                    message = "Similarity matrix built successfully",
                    productsCount = matrix.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building similarity matrix");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}

