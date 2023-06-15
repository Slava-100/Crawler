using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Crawler
{
    public class RabbitLinks
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public RabbitLinks()
        {
            ConnectionFactory factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "links",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
       
        public async Task PublishesLinkAsync(string link)
        {
            var body = Encoding.UTF8.GetBytes(link);

            _channel.BasicPublish(exchange: "",
                                 routingKey: "links",
                                 basicProperties: null,
                                 body: body);
        }

        public async Task ConsumesLinkAsync()
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, e) =>
            {
                var body = e.Body;
                var link = Encoding.UTF8.GetString(body.ToArray());
                await Crawler.GetInstance().ParseAsync(link);
            };

            _channel.BasicConsume(queue: "links",
                                 autoAck: true,
                                 consumer: consumer);
        }

    }
}
