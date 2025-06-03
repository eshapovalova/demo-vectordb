using Grpc.Net.Client;
using Qdrant.Client; // For QdrantClient
using Qdrant.Client.Grpc; // For protobuf types like CollectionInfo, PointStruct, etc.

namespace VectorDbDemo.Services
{
    public class QdrantService
    {
        private readonly QdrantClient _client;

        // public QdrantService(string host, int port)
        // {
        //     // The QdrantClient constructor now expects a GrpcChannel or a host string.
        //     //Example: _client = new QdrantClient("localhost:6334");
        //     _client = new QdrantClient(host, port);
        //     
        //     // var channel = GrpcChannel.ForAddress(host, new GrpcChannelOptions
        //     // {
        //     //     // *** This is the key line to allow insecure HTTP/2 connections ***
        //     //     UnsafeUseInsecureHttp2 = true
        //     // });
        //     // _client = new QdrantClient(channel);
        // }

        public QdrantService(string host, string apiKey)
        {
            _client = new QdrantClient(host: host, https: true, apiKey:apiKey);
        }

        public async Task CreateCollectionAsync(string collectionName, int vectorSize)
        {
            var vectorConfig = new VectorParams
            {
                Size = (ulong)vectorSize, // Use ulong for Size
                Distance = Distance.Cosine
            };

            await _client.CreateCollectionAsync(collectionName, vectorConfig);
        }

        public async Task UpsertPointsAsync(string collectionName, IReadOnlyList<PointStruct> points)
        {
            var upsertPointsRequest = new UpsertPoints
            {
                CollectionName = collectionName,
                Wait = true, // It's often good to wait for operations to complete
                Points = { points } // Add the IEnumerable of points
            };

            await _client.UpsertAsync(collectionName, points, true);
        }

        public async Task<IEnumerable<ScoredPoint>> SearchAsync(
            string collectionName,
            float[] vector,
            int limit = 3,
            Dictionary<string, object>? filter = null)
        {
            var searchParams = new SearchParams
            {
                HnswEf = 128,
                Exact = false
            };

            // var searchPoints = new SearchPoints
            // {
            //     Vector = vector,
            //     Limit = (uint)limit,
            //     WithPayload = true,
            //     WithVector = false,
            //     Filter = filter != null ? new Filter { Must = { new Condition { Field = filter } } } : null
            // };
            //
            // var result = await _client.SearchAsync(collectionName, searchPoints, searchParams);
            // return result;

            //Filter filterSearch = filter != null ? new Filter { Must = { new Condition { Field = new FieldCondition { } }} : null;
            
            var result = await _client.SearchAsync(collectionName, vector, null, searchParams);
            return result;
        }

        public async Task DeleteCollectionAsync(string collectionName)
        {
            await _client.DeleteCollectionAsync(collectionName);
        }
        
        
    }
}