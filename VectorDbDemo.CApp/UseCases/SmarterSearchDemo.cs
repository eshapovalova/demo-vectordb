using Qdrant.Client.Grpc;
using VectorDbDemo.Services;

namespace VectorDbDemo.CApp.UseCases;

public static class SmarterSearchDemo
{
    private const string CollectionName = "articles";
    private static readonly string[] SampleArticles = new[]
    {
        "The latest advancements in quantum computing have shown promising results in solving complex optimization problems.",
        "Machine learning models are revolutionizing the way we process and analyze large datasets in real-time.",
        "Cloud computing has become an essential part of modern software architecture, enabling scalable and flexible solutions."
    };

    public static async Task Run(OpenAIService openAIService, QdrantService qdrantService)
    {
        Console.WriteLine("\nSmarter Search Demo");
        Console.WriteLine("==================");

        try
        {
            // Create collection
            await qdrantService.CreateCollectionAsync(CollectionName, 1536);
            Console.WriteLine("Created collection 'articles'");

            // Generate embeddings and store points
            var embeddings = await openAIService.GetEmbeddingsAsync(SampleArticles);
            var points = new List<PointStruct>();

            for (int i = 0; i < SampleArticles.Length; i++)
            {
                points.Add(new PointStruct
                {
                    Id = (ulong)i,
                    Payload = { ["text"] = SampleArticles[i] },
                    Vectors = embeddings[i]
                });
            }

            await qdrantService.UpsertPointsAsync(CollectionName, points);
            Console.WriteLine("Indexed sample articles");

            // Get search query from user
            Console.Write("\nEnter your search query: ");
            var query = Console.ReadLine() ?? string.Empty;

            // Search for similar articles
            var queryEmbedding = await openAIService.GetEmbeddingAsync(query);
            var results = await qdrantService.SearchAsync(CollectionName, queryEmbedding);

            // Display results
            Console.WriteLine("\nTop matching articles:");
            foreach (var result in results)
            {
                Console.WriteLine($"\nScore: {result.Score:F4}");
                Console.WriteLine($"Text: {result.Payload["text"]}");
            }
        }
        finally
        {
            // Cleanup
            await qdrantService.DeleteCollectionAsync(CollectionName);
        }
    }
}