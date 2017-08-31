using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Threading;
using WpfLiteCoinTester;

public class Listener : RabbitMQMessenger
{
    public Listener(string hostname, string username, string password) : base(hostname, username, password)
    {
    }

    public void Listen(AutoResetEvent mre, Action<object, byte[]> callback, string queue)
    {
        try
        {
            using (var connection = this.factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) => callback.Invoke(model, ea.Body);

                channel.BasicConsume(queue: queue, noAck: true, consumer: consumer);
                mre.WaitOne();
            }
        }
        catch (BrokerUnreachableException exc)
        {
            // log failure to connect to message queue
        }
    }
}
