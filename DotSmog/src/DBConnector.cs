using System.Globalization;
using System.Text;
using System.Text.Json;

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

        var connectionString =
            $"mongodb://{mongoDbUser}:{mongoDbPassword}@{mongoDbHost}:{mongoDbPort}/{mongoDbName}?authSource=admin";
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
    public async Task<List<SensorMessage>> GetDataAsync(
        string collectionName,
        string? stationType = null,
        DateTime? date = null,
        string? stationId = null,
        string? limit = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        var collection = _database.GetCollection<SensorMessage>(collectionName);

        // Lista dozwolonych pól sortowania
        var allowedSortFields = new[] { "MessageUUID", "StationId", "DateTime", "Type", "Value" };

        // Debugowanie parametrów
        Console.WriteLine(
            $"Parameters received: stationType={stationType}, date={date}, stationId={stationId}, limit={limit}, sortBy={sortBy}, sortOrder={sortOrder}");

        // Budowanie filtra
        var filter = Builders<SensorMessage>.Filter.Empty;

        if (!string.IsNullOrEmpty(stationType))
        {
            if (Enum.TryParse(stationType, true, out Type parsedType))
            {
                filter = Builders<SensorMessage>.Filter.Eq(x => x.Type, parsedType.ToString());
            }
            else
            {
                Console.WriteLine($"Invalid stationType value: {stationType}");
            }
        }

        if (date.HasValue)
        {
            var dateWithoutTime = date.Value.Date;
            filter = Builders<SensorMessage>.Filter.And(
                filter,
                Builders<SensorMessage>.Filter.Gte(x => x.DateTime, dateWithoutTime));
        }

        if (!string.IsNullOrEmpty(stationId))
        {
            filter = Builders<SensorMessage>.Filter.And(
                filter,
                Builders<SensorMessage>.Filter.Eq(x => x.StationId, stationId));
        }

        // Tworzenie zapytania
        var query = collection.Find(filter);

        // Obsługa sortowania
        if (!string.IsNullOrEmpty(sortBy))
        {
            if (allowedSortFields.Contains(sortBy))
            {
                var sortDefinition = sortOrder?.ToLower() == "desc"
                    ? Builders<SensorMessage>.Sort.Descending(sortBy)
                    : Builders<SensorMessage>.Sort.Ascending(sortBy);

                query = query.Sort(sortDefinition);
            }
            else
            {
                Console.WriteLine(
                    $"Invalid sortBy field: {sortBy}. Allowed fields are: {string.Join(", ", allowedSortFields)}.");
            }
        }

        // Obsługa limitu
        if (int.TryParse(limit, out var parsedLimit) && parsedLimit > 0)
        {
            query = query.Limit(parsedLimit);
        }
        else if (!string.IsNullOrEmpty(limit))
        {
            Console.WriteLine($"Invalid limit value: {limit}");
        }

        // Pobranie danych
        var documents = await query.ToListAsync();

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
    
    public async Task<Stream> ExportToCsvAsync(List<SensorMessage> data)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        {
            await writer.WriteLineAsync("Id,SensorId,Value,Timestamp,Type");
            foreach (var item in data)
                await writer.WriteLineAsync($"{item.MessageUUID}," +
                                            $"{item.StationId}," +
                                            $"{item.Value.ToString(CultureInfo.InvariantCulture)}," +
                                            $"{item.DateTime:yyyy-MM-ddTHH:mm:ss.fffZ},"+
            $"{item.Type}" );
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<Stream> ExportToJsonAsync(List<SensorMessage> data)
    {
        var memoryStream = new MemoryStream();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        var bytes = Encoding.UTF8.GetBytes(json);
        await memoryStream.WriteAsync(bytes, 0, bytes.Length);
        memoryStream.Position = 0;
        return memoryStream;
    }
}