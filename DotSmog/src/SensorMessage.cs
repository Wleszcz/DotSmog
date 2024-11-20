using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace DotSmog.src
{
    public class Messages
    {
      
        public List<SensorMessage> SensorMessages { get; set; } = new List<SensorMessage>();
    }

    public enum Type
    {
        TYPE1 = 0,
        TYPE2 = 1,
        TYPE3 = 2,
        TYPE4 = 3
    }

    public class SensorMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("messageUUID")]
        public Guid MessageUUID { get; set; }

        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("stationId")]
        public string StationId { get; set; }

        [JsonPropertyName("dateTime")]
        public DateTime DateTime { get; set; }

        [BsonRepresentation(BsonType.String)]  
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }
}
