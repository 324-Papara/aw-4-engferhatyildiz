using System.Text;
using RabbitMQ.Client;

namespace Para.Bussiness.RabbitMq;

public class EmailProducer
{
    public void SendEmailToQueue(string email, string subject, string body)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "email_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = $"{email}|{subject}|{body}";
            var messageBody = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: "email_queue",
                basicProperties: null,
                body: messageBody);
            Console.WriteLine(" [x] Sent {0}", message);
        }
    }
}