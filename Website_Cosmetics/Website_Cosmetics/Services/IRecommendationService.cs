using Website_Cosmetics.Models;

namespace Website_Cosmetics.Services
{
    public interface IRecommendationService
    {
        Task<List<RecommendationResult>> GetRecommendationsAsync(int productId, int topK = 5);
    }

    public class RecommendationResult
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Similarity { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
    }
}


