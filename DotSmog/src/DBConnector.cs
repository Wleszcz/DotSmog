namespace DotSmog;

using MongoDB.Bson;
using MongoDB.Driver;

public class DBConnector
{
    private static DBConnector _instance;
    private static readonly object _lock = new object();

    private readonly IMongoDatabase _database;

    public DBConnector()
    {
        var mongoDbHost = Environment.GetEnvironmentVariable("MongoDB__Host") ?? "localhost";
        var mongoDbPort = Environment.GetEnvironmentVariable("MongoDB__Port") ?? "27017";
        var mongoDbName = Environment.GetEnvironmentVariable("MongoDB__Database") ?? "smogdb";
        var mongoDbUser = Environment.GetEnvironmentVariable("MongoDB__User") ?? "admin";
        var mongoDbPassword = Environment.GetEnvironmentVariable("MongoDB__Password") ?? "password";

        var connectionString = $"mongodb://{mongoDbUser}:{mongoDbPassword}@{mongoDbHost}:{mongoDbPort}/{mongoDbName}?authSource=admin";
        var client = new MongoClient(connectionString);

        // Select database
        _database = client.GetDatabase(mongoDbName);
    }

    // Insert document into collection
    public async Task InsertDataAsync(string collectionName, BsonDocument document)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        await collection.InsertOneAsync(document);
        Console.WriteLine("Document inserted.");
    }

    // Retrieve all documents from collection
    public async Task<List<BsonDocument>> GetDataAsync(string collectionName)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var documents = await collection.Find(new BsonDocument()).ToListAsync();
        Console.WriteLine($"Found {documents.Count} documents.");
        return documents;
    }
    
    public static DBConnector Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new DBConnector();
                }
                return _instance;
            }
        }
    }
}
