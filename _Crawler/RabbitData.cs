using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using _Elastic;
using Nest;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _Crawler
{
    public class RabbitData
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public readonly IElasticClient _elasticClient;
        public RabbitData() 
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "result",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _elasticClient = Elastic.CreateIndex();
        }
        public async Task PublishesDataAsync(NewsModel message)
        {
            var body = Encoding.UTF8.GetBytes(message.ToString());

            _channel.BasicPublish(exchange: "",
                                 routingKey: "result",
                                 basicProperties: null,
                                 body: body);
        }

        public async Task ConsumesDataAsync()
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (sender, e) =>
            {
                var body = e.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                await _elasticClient.IndexDocumentAsync(message);
            };

            _channel.BasicConsume(queue: "result",
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}
