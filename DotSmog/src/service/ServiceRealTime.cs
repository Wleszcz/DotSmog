using System.Collections.Concurrent;
using DotSmog.src;

namespace DotSmog.service;

public class ServiceRealTime
{
    private readonly ConcurrentDictionary<string, List<double>> sensorData = new();
    public BlockingCollection<SensorRealTimeMessage> DataQueue { get; } = new();

    public void AddMessage(SensorMessage message)
    {
        if (!sensorData.ContainsKey(message.StationId))
        {
            sensorData[message.StationId] = new List<double>();
        }

        var values = sensorData[message.StationId];
        values.Add(message.Value);

        if (values.Count > 100)
        {
            values.RemoveAt(0);
        }

        var realTimeMessage = new SensorRealTimeMessage
        {
            MessageUUID = Guid.NewGuid(),
            StationId = message.StationId,
            Type = message.Type,
            LastValue = values.Last(),
            AverageValue = values.Average()
        };

        DataQueue.Add(realTimeMessage);
        Console.WriteLine($"Added message for: {realTimeMessage.StationId}");
    }
}