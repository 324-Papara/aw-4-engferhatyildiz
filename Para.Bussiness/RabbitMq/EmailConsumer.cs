using System.Net;
using System.Net.Mail;
using System.Text;
using Hangfire;
using Hangfire.MemoryStorage;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Para.Bussiness.RabbitMq;

public class EmailConsumer
{
    private readonly IConnectionFactory _factory;

    public EmailConsumer()
    {
        _factory = new ConnectionFactory() { HostName = "localhost" };
    }

    public void Start()
    {
        RecurringJob.AddOrUpdate(() => ConsumeEmailQueue(), "*/5 * * * * *"); // 5 seconds
        Console.WriteLine("Hangfire Server started.");
    }

    public void ConsumeEmailQueue()
    {
        using (var connection = _factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "email_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var emailParts = message.Split('|');
                SendEmail(emailParts[0], emailParts[1], emailParts[2]);
            };

            channel.BasicConsume(queue: "email_queue",
                autoAck: true,
                consumer: consumer);
        }
    }

    private void SendEmail(string email, string subject, string body)
    {
        using (var client = new SmtpClient("smtp.gmail.com", 587))
        {
            client.Credentials = new NetworkCredential("eng.ferhatyildiz@gmail.com", "password");
            client.EnableSsl = true;

            var mailMessage = new MailMessage("eng.ferhatyildiz@gmail.com", email, subject, body);
            client.Send(mailMessage);
        }
    }
}