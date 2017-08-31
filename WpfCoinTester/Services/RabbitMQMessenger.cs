using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Diagnostics;
using System.IO;

namespace WpfLiteCoinTester
{
    public class RabbitMQMessenger
    {
        protected readonly ConnectionFactory factory;

        public RabbitMQMessenger(string hostname, string username, string password)
        {
            this.factory = new ConnectionFactory() { HostName = hostname, UserName = username, Password = password, Port = 5672};
        }

        public bool Publish(string queue, object message)
        {
            try
            {
                using (var connection = this.factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    byte[] body = ConvertToBytes(message);

                    channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
                }

                return true;
            }
            catch(Exception exc)
            {
                var message1 = exc.Message;
                Debug.WriteLine(message1);
            }

            return false;
        }

        public static byte[] ConvertToBytes(object obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new Newtonsoft.Json.Bson.BsonDataWriter(ms))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, new { Value = obj });

                    return ms.ToArray();
                }
            }
        }
    }
}
