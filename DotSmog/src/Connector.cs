using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Connector
{
    private const int RetryDelay = 2000;

    public void StartListening()
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "localhost";
        var rabbitMqUser = Environment.GetEnvironmentVariable("RabbitMQ__UserName") ?? "guest";
        var rabbitMqPass = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest";

        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqHost,
            UserName = rabbitMqUser,
            Password = rabbitMqPass,
        };

        IConnection connection = null;
        int attempts = 0;

        // Connection Attept
        while (true)
        {
            try
            {
                connection = factory.CreateConnection();
                Console.WriteLine("RabbitMQ connection established");
                break; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Retry in {RetryDelay / 1000} sec...");
                Thread.Sleep(RetryDelay); 
            }
        }
        
        using (connection)
        {
            using (var channel = connection.CreateModel())
            {
                string queueName = Environment.GetEnvironmentVariable("RabbitMQ__QueueName") ?? "sensorQueue";

                channel.QueueDeclare(
                    queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"[x] Odebrano wiadomość: {message}");
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                var waitHandle = new ManualResetEvent(false);  
                waitHandle.WaitOne(); 
            }
        }
    }
}
