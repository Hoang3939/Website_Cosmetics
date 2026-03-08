using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Data;
using Website_Cosmetics.Models;
using Newtonsoft.Json;

namespace Website_Cosmetics.Services
{
    public class RAGService : IRAGService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RAGService> _logger;
        
        // In-memory conversation history (key: sessionId, value: deque of (user, bot) tuples)
        private static readonly ConcurrentDictionary<string, Queue<(string, string)>> _conversationHistory = new();
        
        // Cache for product embeddings (key: productId, value: embedding array)
        private static readonly ConcurrentDictionary<int, float[]> _embeddingCache = new();

        public RAGService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<RAGService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ChatResponse> ProcessMessageAsync(string userMessage, string? sessionId = null)
        {
            sessionId ??= Guid.NewGuid().ToString();
            
            // Get conversation history
            var history = _conversationHistory.GetOrAdd(sessionId, _ => new Queue<(string, string)>(5));
            var historyList = history.ToList();

            // Step 1: Decide action (RAG or Chat)
            var decision = await DecideActionAsync(userMessage, historyList);
            _logger.LogInformation($"Decision: {decision.Action} - {decision.Reason}");

            string response;
            List<ProductInfo> products = new();

            if (decision.Action == "rag")
            {
                // Step 2: Retrieve relevant products (filter by max price $35 = 875,000 VND)
                const decimal maxPriceUSD = 35;
                const decimal maxPriceVND = maxPriceUSD * 25000; // Approximate conversion
                var retrievedProducts = await RetrieveProductsAsync(userMessage, topK: 5);
                
                // Filter products by price (max $35)
                var filteredProducts = retrievedProducts
                    .Where(p => p.BasePrice <= maxPriceVND)
                    .ToList();
                
                // Step 3: Generate response with context
                var productContext = string.Join("\n", filteredProducts.Select(p => 
                {
                    var desc = p.Description ?? "";
                    var descPreview = desc.Length > 100 ? desc.Substring(0, 100) + "..." : desc;
                    var priceUSD = p.BasePrice / 25000; // Convert to USD
                    return $"- {p.Name} - {descPreview} Price: ${priceUSD:F2}";
                }));
                
                var prompt = $@"User asked: {userMessage}
Here are relevant products (all under $35):
{productContext}

Answer naturally in English. Be helpful and concise (1-2 sentences).";

                response = await ChatWithGeminiAsync(prompt);
                
                // Use already filtered products (max $35) - double check price filter
                products = filteredProducts
                    .Where(p => p.BasePrice <= maxPriceVND) // Double check filter using existing variable
                    .Select(p => new ProductInfo
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Price = p.BasePrice,
                        Similarity = 0.0, // Will be calculated if needed
                        Description = p.Description,
                        Slug = p.Slug,
                        ImageUrl = null // No images needed
                    }).ToList();
            }
            else
            {
                // Step 2: Direct chat
                response = await ChatWithGeminiAsync(userMessage);
            }

            // Update conversation history
            if (history.Count >= 5)
            {
                history.Dequeue();
            }
            history.Enqueue((userMessage, response));

            return new ChatResponse
            {
                Response = response,
                Action = decision.Action,
                Reason = decision.Reason,
                Products = products
            };
        }

        public async Task<List<Product>> RetrieveProductsAsync(string query, int topK = 5)
        {
            try
            {
                // Generate query embedding
                var queryEmbedding = await GenerateEmbeddingAsync(query, isQuery: true);
                if (queryEmbedding == null || queryEmbedding.Length == 0)
                {
                    _logger.LogWarning("Failed to generate query embedding");
                    return new List<Product>();
                }

                // Filter by max price $35 = 875,000 VND from the start
                const decimal maxPriceVND = 35 * 25000; // $35 = 875,000 VND
                
                // Get all products with embeddings, filtered by price
                var products = await _context.Products
                    .Where(p => p.Embedding != null && p.IsActive == true && p.BasePrice <= maxPriceVND)
                    .ToListAsync();

                if (products.Count == 0)
                {
                    return new List<Product>();
                }

                // Calculate similarities
                var similarities = new List<(Product product, double similarity)>();

                foreach (var product in products)
                {
                    var productEmbedding = await GetProductEmbeddingAsync(product.ProductId, product.Embedding);
                    if (productEmbedding != null && productEmbedding.Length > 0)
                    {
                        var similarity = CosineSimilarity(queryEmbedding, productEmbedding);
                        similarities.Add((product, similarity));
                    }
                }

                // Get top K products
                var topProducts = similarities
                    .OrderByDescending(s => s.similarity)
                    .Take(topK)
                    .Select(s => s.product)
                    .ToList();

                return topProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return new List<Product>();
            }
        }

        public async Task<bool> GenerateEmbeddingsForAllProductsAsync()
        {
            try
            {
                var products = await _context.Products
                    .Where(p => p.Description != null && p.Description != string.Empty)
                    .ToListAsync();

                _logger.LogInformation($"Generating embeddings for {products.Count} products");

                foreach (var product in products)
                {
                    await GenerateEmbeddingForProductAsync(product.ProductId);
                    await Task.Delay(100); // Rate limiting
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embeddings for all products");
                return false;
            }
        }

        public async Task<bool> GenerateEmbeddingForProductAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null || string.IsNullOrEmpty(product.Description))
                {
                    return false;
                }

                var embedding = await GenerateEmbeddingAsync(product.Description, isQuery: false);
                if (embedding == null || embedding.Length == 0)
                {
                    return false;
                }

                // Store as JSON string
                product.Embedding = JsonConvert.SerializeObject(embedding);
                _embeddingCache.TryRemove(productId, out _); // Clear cache

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating embedding for product {productId}");
                return false;
            }
        }

        private async Task<(string Action, string? Reason)> DecideActionAsync(string userMessage, List<(string, string)> history)
        {
            try
            {
                var summary = history.Count > 0
                    ? string.Join("\n", history.Select((h, i) => $"{i + 1}. User: {h.Item1}\n   Bot: {h.Item2}"))
                    : "No previous conversation.";

                var prompt = $@"You are a decision-making assistant for a website chatbot.

Your goal: decide whether the chatbot should use **RAG** (retrieve data) or **CHAT** (casual conversation).

---

### Output format:
{{
  ""action"": ""rag"" | ""chat"",
  ""reason"": ""short explanation in Vietnamese or English""
}}

---

### Decision rules:

Choose **""rag""** if:
- The user asks for information, products, or new data.
- The user applies filters (e.g., ""dưới 500k"", ""màu đỏ"", ""size L"").
- The user requests details or comparisons from external data.

Choose **""chat""** if:
- The user reacts, agrees, or continues a natural conversation.
- The user comments about shown items (e.g., ""ừ, mẫu A đẹp đó"").
- The user greets, thanks, or makes casual remarks.
- If unsure, default to ""chat"".

---

### Conversation Summary:
{summary}

### User Message:
{userMessage}

Return only a valid JSON object as specified above.";

                var apiKey = _configuration["Gemini:DecisionApiKey"] ?? _configuration["Gemini:EmbedApiKey"] ?? "";
                var response = await CallGeminiAPIAsync(
                    apiKey: apiKey,
                    model: "gemini-2.5-flash",
                    prompt: prompt,
                    temperature: 0,
                    responseMimeType: "application/json"
                );

                if (string.IsNullOrEmpty(response))
                {
                    return ("chat", "fallback: no response");
                }

                var decision = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                if (decision != null && decision.ContainsKey("action"))
                {
                    return (decision["action"], decision.GetValueOrDefault("reason"));
                }

                return ("chat", "fallback: invalid response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in decision making");
                return ("chat", "fallback: error");
            }
        }

        private async Task<string> ChatWithGeminiAsync(string message)
        {
            try
            {
                var systemContext = @"You are a friendly and helpful chatbot assistant on a cosmetics shopping website.
- Always reply in a short, natural, and conversational way in English.
- Use simple and clear language.
- Keep responses concise (1–2 sentences).
- Use a polite, friendly tone.";

                var fullPrompt = $"{systemContext}\nUser: {message}\nChatbot:";

                var apiKey = _configuration["Gemini:ChatApiKey"] ?? _configuration["Gemini:EmbedApiKey"] ?? "";
                var response = await CallGeminiAPIAsync(
                    apiKey: apiKey,
                    model: "gemini-2.5-flash",
                    prompt: fullPrompt,
                    temperature: 0.7
                );

                return response ?? "Sorry, I cannot answer this question right now.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat");
                return "Sorry, an error occurred. Please try again later.";
            }
        }

        private async Task<float[]?> GenerateEmbeddingAsync(string text, bool isQuery)
        {
            try
            {
                var apiKey = _configuration["Gemini:EmbedApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Gemini Embed API key not configured");
                    return null;
                }

                var taskType = isQuery ? "RETRIEVAL_QUERY" : "RETRIEVAL_DOCUMENT";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

                var requestBody = new
                {
                    model = "models/text-embedding-004",
                    content = new { parts = new[] { new { text } } },
                    taskType = taskType
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent",
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                
                if (result?.embedding?.values != null)
                {
                    var values = ((Newtonsoft.Json.Linq.JArray)result.embedding.values)
                        .Select(v => (float)v)
                        .ToArray();
                    return values;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding");
                return null;
            }
        }

        private async Task<float[]?> GetProductEmbeddingAsync(int productId, string? embeddingJson)
        {
            // Check cache first
            if (_embeddingCache.TryGetValue(productId, out var cached))
            {
                return await Task.FromResult<float[]?>(cached);
            }

            if (string.IsNullOrEmpty(embeddingJson))
            {
                return null;
            }

            try
            {
                var embedding = JsonConvert.DeserializeObject<float[]>(embeddingJson);
                if (embedding != null)
                {
                    _embeddingCache.TryAdd(productId, embedding);
                }
                return await Task.FromResult<float[]?>(embedding);
            }
            catch
            {
                return null;
            }
        }

        private double CosineSimilarity(float[] vec1, float[] vec2)
        {
            if (vec1.Length != vec2.Length)
                return 0;

            double dotProduct = 0;
            double norm1 = 0;
            double norm2 = 0;

            for (int i = 0; i < vec1.Length; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        private async Task<string> CallGeminiAPIAsync(
            string apiKey,
            string model,
            string prompt,
            double temperature = 0.7,
            string? responseMimeType = null)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Gemini API key not configured");
                    return string.Empty;
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = temperature,
                        responseMimeType = responseMimeType
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Try v1beta first, fallback to v1 if needed
                // Try v1 first, fallback to v1beta if needed
                var response = await client.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent",
                    content
                );
                
                // If v1 fails, try v1beta
                if (!response.IsSuccessStatusCode)
                {
                    response = await client.PostAsync(
                        $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent",
                        content
                    );
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API error: {response.StatusCode} - {errorContent}");
                    return string.Empty;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseJson);
                
                return result?.candidates?[0]?.content?.parts?[0]?.text?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return string.Empty;
            }
        }
    }
}

