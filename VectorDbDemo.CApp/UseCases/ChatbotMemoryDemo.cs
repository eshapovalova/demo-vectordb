using Qdrant.Client.Grpc;
using VectorDbDemo.Services;

namespace VectorDbDemo.CApp.UseCases;

public static class ChatbotMemoryDemo
{
    private const string CollectionName = "chat_memory";
    private static readonly string[] SampleMessages = new[]
    {
        "I'm having trouble with my internet connection. It keeps dropping randomly.",
        "The new software update is causing my computer to run slowly.",
        "How do I reset my password for the company portal?",
        "I need help setting up my email on my new phone.",
        "The printer in the office is showing an error code E-05."
    };

    public static async Task Run(OpenAIService openAIService, QdrantService qdrantService)
    {
        Console.WriteLine("\nChatbot Memory Demo");
        Console.WriteLine("==================");

        try
        {
            // Create collection
            await qdrantService.CreateCollectionAsync(CollectionName, 1536);
            Console.WriteLine("Created collection 'chat_memory'");

            // Generate embeddings and store points
            var embeddings = await openAIService.GetEmbeddingsAsync(SampleMessages);
            var points = new List<PointStruct>();

            for (int i = 0; i < SampleMessages.Length; i++)
            {
                points.Add(new PointStruct
                {
                    Id = (ulong)i,
                    Payload = {
                        {
                            "message", SampleMessages[i]
                        }, 
                        {
                            "timestamp", DateTime.UtcNow.AddMinutes(-i).ToString("o")
                        }
                    },
                    Vectors = embeddings[i]
                });
            }

            await qdrantService.UpsertPointsAsync(CollectionName, points);
            Console.WriteLine("Indexed sample chat messages");

            // Get new question from user
            Console.Write("\nEnter your question: ");
            var question = Console.ReadLine() ?? string.Empty;

            // Search for relevant past messages
            var questionEmbedding = await openAIService.GetEmbeddingAsync(question);
            var results = await qdrantService.SearchAsync(CollectionName, questionEmbedding);

            // Display results
            Console.WriteLine("\nRelevant past messages:");
            foreach (var result in results)
            {
                Console.WriteLine($"\nRelevance: {result.Score:F4}");
                Console.WriteLine($"Message: {result.Payload["message"]}");
                Console.WriteLine($"Time: {result.Payload["timestamp"]}");
            }
        }
        finally
        {
            // Cleanup
            await qdrantService.DeleteCollectionAsync(CollectionName);
        }
    }
} 