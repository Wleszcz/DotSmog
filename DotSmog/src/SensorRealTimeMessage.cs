using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace DotSmog.src
{
    // public class Messages
    // {
      
    //     public List<SensorRealTimeMessage> SensorRealTimeMessages { get; set; } = new List<SensorRealTimeMessage>();
    // }

    // public enum Type
    // {
    //     TYPE1 = 0,
    //     TYPE2 = 1,
    //     TYPE3 = 2,
    //     TYPE4 = 3
    // }

    public class SensorRealTimeMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("messageUUID")]
        public Guid MessageUUID { get; set; }

        [BsonRepresentation(BsonType.String)]
        [JsonPropertyName("stationId")]
        public string StationId { get; set; }

        [BsonRepresentation(BsonType.String)]  
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("lastValue")]
        public double LastValue { get; set; }

        [JsonPropertyName("averageValue")]
        public double AverageValue { get; set; }
    }
}
