namespace DotSmog;

using DotSmog.src;
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
    public async Task InsertDataAsync(string collectionName, SensorMessage document)
    {
        var collection = _database.GetCollection<SensorMessage>(collectionName);
        await collection.InsertOneAsync(document);
        Console.WriteLine("Document inserted.");
    }

    // Retrieve all documents from collection
    public async Task<List<SensorMessage>> GetDataAsync(string collectionName, string? stationType = null, DateTime? date = null,
    Guid? stationUUID = null)
    {
        var collection = _database.GetCollection<SensorMessage>(collectionName);

        // Jeœli `stationType` jest podany, dodajemy filtr
        var filter = Builders<SensorMessage>.Filter.Empty;

        if (!string.IsNullOrEmpty(stationType))
        {
            if (Enum.TryParse(stationType, true, out Type parsedType)) 
            {
                filter = Builders<SensorMessage>.Filter.Eq(x => x.Type, parsedType.ToString());
            }
            else
            {
                Console.WriteLine($"Invalid station type: {stationType}");
            }
        }
        if (date.HasValue)
        {
            var dateWithoutTime = date.Value.Date; // Date ustawia godzinê na 00:00:00
            filter = Builders<SensorMessage>.Filter.And(
                filter,
                Builders<SensorMessage>.Filter.Gte(x => x.DateTime, dateWithoutTime));
        }
        if (stationUUID.HasValue)
        {
            filter = Builders<SensorMessage>.Filter.And(
                filter,
                Builders<SensorMessage>.Filter.Eq(x => x.StationUUID, stationUUID.Value));
        }
        var documents = await collection.Find(filter).ToListAsync();
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
