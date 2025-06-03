using Qdrant.Client.Grpc;
using VectorDbDemo.Services;

namespace VectorDbDemo.CApp.UseCases;

public static class AnomalyDetectionDemo
{
    private const string CollectionName = "logs";
    private const double AnomalyThreshold = 0.7; // Higher threshold means more strict anomaly detection

    private static readonly string[] NormalLogs = new[]
    {
        "User login successful: john.doe@company.com",
        "File upload completed: report.pdf",
        "Database backup completed successfully",
        "API request processed: GET /api/users",
        "Email sent successfully to: support@company.com",
        "System health check passed",
        "Cache cleared successfully",
        "User profile updated: jane.smith@company.com"
    };

    private static readonly string[] AnomalyLogs = new[]
    {
        "Multiple failed login attempts detected for admin account",
        "Database connection timeout after 30 seconds",
        "Critical security vulnerability detected in system",
        "Unauthorized access attempt to restricted API endpoint",
        "System memory usage exceeded 95% threshold"
    };

    public static async Task Run(OpenAIService openAIService, QdrantService qdrantService)
    {
        Console.WriteLine("\nAnomaly Detection Demo");
        Console.WriteLine("=====================");

        try
        {
            // Create collection
            await qdrantService.CreateCollectionAsync(CollectionName, 1536);
            Console.WriteLine("Created collection 'logs'");

            // Generate embeddings and store normal logs
            var embeddings = await openAIService.GetEmbeddingsAsync(NormalLogs);
            var points = new List<PointStruct>();

            for (int i = 0; i < NormalLogs.Length; i++)
            {
                points.Add(new PointStruct
                {
                    Id = (ulong)i,
                    Payload = {
                        {
                            "message", NormalLogs[i]
                        }, 
                        {
                            "timestamp", DateTime.UtcNow.AddMinutes(-i).ToString("o")
                        }
                    },
                    Vectors = embeddings[i]
                });
            }

            await qdrantService.UpsertPointsAsync(CollectionName, points);
            Console.WriteLine("Indexed normal log entries");

            // Test anomaly detection
            Console.WriteLine("\nTesting anomaly detection with sample logs:");
            foreach (var log in AnomalyLogs)
            {
                var logEmbedding = await openAIService.GetEmbeddingAsync(log);
                var results = await qdrantService.SearchAsync(CollectionName, logEmbedding, limit: 1);
                
                var similarity = results.First().Score;
                var isAnomaly = similarity < AnomalyThreshold;

                Console.WriteLine($"\nLog: {log}");
                Console.WriteLine($"Similarity Score: {similarity:F4}");
                Console.WriteLine($"Anomaly Detected: {(isAnomaly ? "YES" : "NO")}");
                
                if (isAnomaly)
                {
                    Console.WriteLine("ALERT: Potential security or system issue detected!");
                }
            }

            // Test with user input
            Console.Write("\nEnter a log message to test: ");
            var userLog = Console.ReadLine() ?? string.Empty;

            var userLogEmbedding = await openAIService.GetEmbeddingAsync(userLog);
            var userResults = await qdrantService.SearchAsync(CollectionName, userLogEmbedding, limit: 1);
            
            var userSimilarity = userResults.First().Score;
            var isUserLogAnomaly = userSimilarity < AnomalyThreshold;

            Console.WriteLine($"\nSimilarity Score: {userSimilarity:F4}");
            Console.WriteLine($"Anomaly Detected: {(isUserLogAnomaly ? "YES" : "NO")}");
            
            if (isUserLogAnomaly)
            {
                Console.WriteLine("ALERT: Potential security or system issue detected!");
            }
        }
        finally
        {
            // Cleanup
            await qdrantService.DeleteCollectionAsync(CollectionName);
        }
    }
} 