using RabbitMQ.Client;
using System.Text;
using System;

public class Pusher
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Pusher));

    public void Push(ConnectionFactory factory, string queue, string message)
    {
        try
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

                byte[] body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
            }
        }
        catch(Exception exc)
        {
            log.ErrorFormat("Error pushing to queue: {0}\n{1}\n{2}", queue, exc.Message, exc.StackTrace);
        }
    }
}
