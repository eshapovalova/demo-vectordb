using OpenAI;
using OpenAI.Embeddings;

namespace VectorDbDemo.Services;

public class OpenAIService(string apiKey)
{
    #region Fields

    private readonly OpenAIClient _client = new(apiKey);
    private readonly EmbeddingClient _embeddingClient = new(Model, apiKey);

    private const string Model = "text-embedding-3-small";
    private const int EmbeddingSize = 1536;

    #endregion
    
    
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // For text-embedding-3-small, the default dimension is 1536.
        // If you explicitly want to limit the dimensions (e.g., to 512 or 256 for storage/performance),
        // var options = new EmbeddingGenerationOptions
        // {
        //     Dimensions = EmbeddingSize
        // };
        
        OpenAIEmbedding response = await _embeddingClient.GenerateEmbeddingAsync(text);
        return response.ToFloats().ToArray();
    }

    public async Task<float[][]> GetEmbeddingsAsync(IEnumerable<string> texts)
    {
        var embeddings = new List<float[]>();
        foreach (var text in texts)
        {
            var embedding = await GetEmbeddingAsync(text);
            embeddings.Add(embedding);
        }
        return embeddings.ToArray();
    }
}