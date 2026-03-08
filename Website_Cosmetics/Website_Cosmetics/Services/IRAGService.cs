using Website_Cosmetics.Models;

namespace Website_Cosmetics.Services
{
    public interface IRAGService
    {
        Task<ChatResponse> ProcessMessageAsync(string userMessage, string? sessionId = null);
        Task<List<Product>> RetrieveProductsAsync(string query, int topK = 5);
        Task<bool> GenerateEmbeddingsForAllProductsAsync();
        Task<bool> GenerateEmbeddingForProductAsync(int productId);
    }

    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public string Action { get; set; } = "chat"; // "rag" or "chat"
        public string? Reason { get; set; }
        public List<ProductInfo> Products { get; set; } = new();
    }

    public class ProductInfo
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Similarity { get; set; }
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
    }
}

