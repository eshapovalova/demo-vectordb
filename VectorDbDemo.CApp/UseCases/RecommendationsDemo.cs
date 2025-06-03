using Qdrant.Client.Grpc;
using VectorDbDemo.Services;

namespace VectorDbDemo.CApp.UseCases;

public static class RecommendationsDemo
{
    private const string CollectionName = "products";
    private static readonly (string Name, string Category)[] SampleProducts = new[]
    {
        ("iPhone 14 Pro", "Electronics"),
        ("Samsung 4K Smart TV", "Electronics"),
        ("Nike Air Max", "Sports"),
        ("Yoga Mat", "Sports"),
        ("Coffee Maker", "Home"),
        ("Blender", "Home")
    };

    public static async Task Run(OpenAIService openAIService, QdrantService qdrantService)
    {
        Console.WriteLine("\nPersonalized Recommendations Demo");
        Console.WriteLine("===============================");

        try
        {
            // Create collection
            await qdrantService.CreateCollectionAsync(CollectionName, 1536);
            Console.WriteLine("Created collection 'products'");

            // Generate embeddings and store points
            var productTexts = SampleProducts.Select(p => $"{p.Name} - {p.Category}");
            var embeddings = await openAIService.GetEmbeddingsAsync(productTexts);
            var points = new List<PointStruct>();

            for (int i = 0; i < SampleProducts.Length; i++)
            {
                points.Add(new PointStruct
                {
                    Id = (ulong)i,
                    Payload = {
                        {
                            "name", SampleProducts[i].Name 
                        }, 
                        {
                            "category", SampleProducts[i].Category 
                        }
                    },
                    Vectors = embeddings[i]
                });
            }

            await qdrantService.UpsertPointsAsync(CollectionName, points);
            Console.WriteLine("Indexed sample products");

            // Simulate user preference
            Console.Write("\nEnter your preferred category (Electronics/Sports/Home): ");
            var category = Console.ReadLine() ?? string.Empty;

            // Generate embedding for category preference
            var categoryEmbedding = await openAIService.GetEmbeddingAsync(category);
            
            // Search for similar products in the preferred category
            var filter = new Dictionary<string, object>
            {
                { "category", category }
            };

            var results = await qdrantService.SearchAsync(CollectionName, categoryEmbedding, filter: filter);

            // Display results
            Console.WriteLine($"\nTop recommendations for {category}:");
            foreach (var result in results)
            {
                Console.WriteLine($"\nScore: {result.Score:F4}");
                Console.WriteLine($"Product: {result.Payload["name"]}");
                Console.WriteLine($"Category: {result.Payload["category"]}");
            }
        }
        finally
        {
            // Cleanup
            await qdrantService.DeleteCollectionAsync(CollectionName);
        }
    }
} 