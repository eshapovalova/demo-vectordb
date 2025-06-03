# Vector Examples

This .NET 9 console application demonstrates four AI-powered use cases using OpenAI embeddings and Qdrant vector database.

## Prerequisites

- .NET 9 SDK
- OpenAI API key
- Qdrant vector database (accessible via URL - free cluster will be enough)

## Setup

1. Clone the repository
2. Update the OpenAI API key in `appsettings.json`
3. Ensure Qdrant is running and accessible
4. Run the application:
   ```bash
   dotnet run
   ```

## Use Cases

### 1. Smarter Search
Demonstrates semantic search functionality by:
- Creating a collection of sample articles
- Converting articles to embeddings
- Finding semantically similar articles based on user queries

### 2. Personalized Recommendations
Shows how to implement product recommendations by:
- Storing products with categories
- Using user preferences to find relevant products
- Filtering results by category

### 3. Chatbot Memory
Illustrates chatbot context retrieval by:
- Storing past conversation messages
- Finding relevant context for new questions
- Maintaining conversation history with timestamps

### 4. Anomaly Detection in Logs
Demonstrates log analysis by:
- Storing normal system logs
- Comparing new logs against normal patterns
- Detecting anomalies based on similarity scores

## Architecture

The application is organized into:
- `VectorDbDemo.Services`: Core services for OpenAI and Qdrant interactions (library)
- `VectorDbDemo.CApp.UseCases`: Implementation of each demo use case
- `VectorDbDemo.CApp >> Program.cs`: Main entry point with menu system

## Dependencies

- OpenAI SDK (2.1.0) 
- Qdrant.Client (1.14.0)

## Notes

- Each use case creates and cleans up its own Qdrant collection
- The application uses the `text-embedding-3-small` model for embeddings
- Vector size is set to 1536 dimensions (3-small model)
- Cosine similarity is used for vector comparisons 