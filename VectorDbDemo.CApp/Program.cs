using Microsoft.Extensions.Configuration;
using VectorDbDemo.CApp.UseCases;
using VectorDbDemo.Services;

namespace VectorDbDemo.CApp;

class Program
{
    private static OpenAIService _openAIService;
    private static QdrantService _qdrantService;

    
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting VectorDbDemo Console App...");

        if (BuildConfiguration(out var openAiApiKey, out var qdrantHost, out var qdrantApiKey)) return; // Exit if critical config is missing

        // 3. Initialize your Services
        _openAIService = new OpenAIService(openAiApiKey);
        _qdrantService = new QdrantService(qdrantHost, qdrantApiKey);
        
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Vector Examples Demo");
            Console.WriteLine("===================");
            Console.WriteLine("1. Smarter Search");
            Console.WriteLine("2. Personalized Recommendations");
            Console.WriteLine("3. Chatbot Memory");
            Console.WriteLine("4. Anomaly Detection in Logs");
            Console.WriteLine("0. Exit");
            Console.Write("\nSelect a use case (0-4): ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input. Press any key to continue...");
                Console.ReadKey();
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0:
                        return;
                    case 1:
                        await SmarterSearchDemo.Run(_openAIService, _qdrantService);
                        break;
                    case 2:
                        await RecommendationsDemo.Run(_openAIService, _qdrantService);
                        break;
                    case 3:
                        await ChatbotMemoryDemo.Run(_openAIService, _qdrantService);
                        break;
                    case 4:
                        await AnomalyDetectionDemo.Run(_openAIService, _qdrantService);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    private static bool BuildConfiguration(out string? openAiApiKey, out string? qdrantHost, out string? qdrantApiKey)
    {
        // 1. Build Configuration
        // This sets up where to find your appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Retrieve API Key and Qdrant Host from Configuration
        openAiApiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(openAiApiKey))
        {
            Console.WriteLine("Error: OpenAI API Key not found in appsettings.json or environment variables.");
            qdrantHost = null;
            qdrantApiKey = null;
            return true;
        }

        qdrantHost = configuration["QdrantCloud:Host"];
        qdrantApiKey = configuration["QdrantCloud:ApiKey"];
        if (string.IsNullOrEmpty(qdrantHost))
        {
            Console.WriteLine("Error: Qdrant Host not found in appsettings.json or environment variables.");
            return true;
        }

        return false;
    }
}