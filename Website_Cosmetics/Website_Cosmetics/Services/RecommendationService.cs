using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Newtonsoft.Json;

namespace Website_Cosmetics.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationService> _logger;
        
        // Cache for ingredient vectors (key: productId, value: ingredient vector)
        private static readonly ConcurrentDictionary<int, Dictionary<string, int>> _ingredientVectors = new();
        
        // Cache for similarity matrix (key: productId, value: list of similar products)
        private static readonly ConcurrentDictionary<int, List<ProductRecommendation>> _similarityCache = new();

        public RecommendationService(
            ApplicationDbContext context,
            ILogger<RecommendationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProductRecommendation>> GetRecommendationsAsync(int productId, int topK = 5)
        {
            try
            {
                // Check cache first
                if (_similarityCache.TryGetValue(productId, out var cached))
                {
                    return cached.Take(topK).ToList();
                }

                // Get the target product
                var targetProduct = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsActive == true);

                if (targetProduct == null || string.IsNullOrEmpty(targetProduct.Ingredients))
                {
                    return new List<ProductRecommendation>();
                }

                // Get all products with ingredients (excluding the target product)
                var allProducts = await _context.Products
                    .Include(p => p.ProductImages)
                    .Where(p => p.ProductId != productId 
                        && p.IsActive == true 
                        && !string.IsNullOrEmpty(p.Ingredients))
                    .ToListAsync();

                if (allProducts.Count == 0)
                {
                    return new List<ProductRecommendation>();
                }

                // Tokenize ingredients for all products
                var allIngredientTokens = new Dictionary<int, HashSet<string>>();
                
                // Add target product
                var targetTokens = TokenizeIngredients(targetProduct.Ingredients);
                allIngredientTokens[productId] = targetTokens;

                // Add other products
                foreach (var product in allProducts)
                {
                    if (!string.IsNullOrEmpty(product.Ingredients))
                    {
                        allIngredientTokens[product.ProductId] = TokenizeIngredients(product.Ingredients);
                    }
                }

                // Build ingredient vocabulary (all unique ingredients)
                var vocabulary = new HashSet<string>();
                foreach (var tokens in allIngredientTokens.Values)
                {
                    vocabulary.UnionWith(tokens);
                }

                // Create one-hot encoded vectors
                var targetVector = CreateOneHotVector(targetTokens, vocabulary);
                var similarities = new List<(Product product, double similarity)>();

                foreach (var product in allProducts)
                {
                    var productVector = CreateOneHotVector(allIngredientTokens[product.ProductId], vocabulary);
                    var similarity = CosineSimilarity(targetVector, productVector);
                    similarities.Add((product, similarity));
                }

                // Get top K recommendations
                var recommendations = similarities
                    .OrderByDescending(s => s.similarity)
                    .Take(topK)
                    .Select(s => new ProductRecommendation
                    {
                        ProductId = s.product.ProductId,
                        Name = s.product.Name,
                        Price = s.product.BasePrice,
                        Similarity = s.similarity,
                        Slug = s.product.Slug,
                        ImageUrl = s.product.ProductImages
                            .Where(img => img.ImageType == "cover" || img.ImageType == "gallery")
                            .OrderBy(img => img.DisplayOrder)
                            .FirstOrDefault()?.Url
                    })
                    .ToList();

                // Cache the results
                _similarityCache.TryAdd(productId, recommendations);

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting recommendations for product {productId}");
                return new List<ProductRecommendation>();
            }
        }

        public async Task<Dictionary<int, List<ProductRecommendation>>> BuildSimilarityMatrixAsync()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.IsActive == true && !string.IsNullOrEmpty(p.Ingredients))
                    .ToListAsync();

                var matrix = new Dictionary<int, List<ProductRecommendation>>();

                foreach (var product in products)
                {
                    var recommendations = await GetRecommendationsAsync(product.ProductId, topK: 10);
                    matrix[product.ProductId] = recommendations;
                }

                return matrix;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building similarity matrix");
                return new Dictionary<int, List<ProductRecommendation>>();
            }
        }

        private HashSet<string> TokenizeIngredients(string ingredients)
        {
            if (string.IsNullOrEmpty(ingredients))
                return new HashSet<string>();

            // Clean and normalize ingredients
            var cleaned = ingredients.ToLower();
            
            // Remove common prefixes and suffixes
            cleaned = Regex.Replace(cleaned, @"\b(water|aqua|eau)\b", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\b(and|&|,|;|:)\b", " ", RegexOptions.IgnoreCase);
            
            // Split by common separators
            var tokens = cleaned
                .Split(new[] { ',', ';', ':', '\n', '\r', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t) && t.Length > 2) // Filter out very short tokens
                .ToHashSet();

            return tokens;
        }

        private Dictionary<string, int> CreateOneHotVector(HashSet<string> tokens, HashSet<string> vocabulary)
        {
            var vector = new Dictionary<string, int>();
            foreach (var ingredient in vocabulary)
            {
                vector[ingredient] = tokens.Contains(ingredient) ? 1 : 0;
            }
            return vector;
        }

        private double CosineSimilarity(Dictionary<string, int> vec1, Dictionary<string, int> vec2)
        {
            if (vec1.Count != vec2.Count)
                return 0;

            double dotProduct = 0;
            double norm1 = 0;
            double norm2 = 0;

            foreach (var key in vec1.Keys)
            {
                var val1 = vec1[key];
                var val2 = vec2.ContainsKey(key) ? vec2[key] : 0;
                
                dotProduct += val1 * val2;
                norm1 += val1 * val1;
                norm2 += val2 * val2;
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }
}

