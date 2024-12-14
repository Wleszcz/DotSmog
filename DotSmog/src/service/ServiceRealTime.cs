using System.Collections.Concurrent;
using DotSmog.src;

namespace DotSmog.service;

public class ServiceRealTime
{
    public BlockingCollection<SensorMessage> DataQueue { get; } = new();

    public void AddMessage(SensorMessage message)
    {
        DataQueue.Add(message);
        Console.WriteLine($"Add message: {message.StationId}");
    }
}