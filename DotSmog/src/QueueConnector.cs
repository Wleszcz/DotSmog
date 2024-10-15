using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace DotSmog
{
    public class QueueConnector
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly int _retryDelay;

        public QueueConnector( int retryDelay = 5000)
        {
            _queueName = Environment.GetEnvironmentVariable("RabbitMQ__QueueName") ?? "sensorQueue";
            _retryDelay = retryDelay;

            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RabbitMQ__HostName") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RabbitMQ__UserName") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest",
            };

            _connection = CreateConnection(factory);
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private IConnection CreateConnection(ConnectionFactory factory)
        {
            while (true)
            {
                try
                {
                    var connection = factory.CreateConnection();
                    Console.WriteLine("RabbitMQ connection established");
                    return connection;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection failed: {ex.Message}. Retry in {_retryDelay / 1000} sec...");
                    Thread.Sleep(_retryDelay);
                }
            }
        }

        public void StartReceiving(CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");


                var dbConnector = DBConnector.Instance;

                try
                {
                    BsonDocument newDocument = BsonDocument.Parse(message);
                    await ProcessMessageAsync(dbConnector, newDocument);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName,
                autoAck: false,
                consumer: consumer);

            // Allow cancellation of message processing
            cancellationToken.Register(() => StopReceiving());
        }

        private async Task ProcessMessageAsync(DBConnector dbConnector, BsonDocument newDocument)
        {
            // Assuming this is an asynchronous operation
            await dbConnector.InsertDataAsync("readings", newDocument);
        }

        public void StopReceiving()
        {
            _channel.Close();
            _connection.Close();
            Console.WriteLine("RabbitMQ connection closed");
        }
    }
}