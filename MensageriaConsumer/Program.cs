using System;
using System.Net.Http;
using System.Text;
using System.Threading.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MensageriaConsumer
{
    internal class Program
    {
        private static readonly string QueueName = "EnviarRemessa";

        static void Main()
        {


            var factory = new ConnectionFactory { HostName = "localhost", Port = 32790 };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // declare a server-named queue
            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine(" [*] Waiting for logs.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] {message}");
                using (var httpClient = new HttpClient())
                {
                    var endpointUrl = "https://localhost:7273/api/Boletos/LerRemessa"; // URL do endpoint da API principal

                    var content = new StringContent(message, Encoding.UTF8, "text/plain");

                    var response = httpClient.PostAsync(endpointUrl, content).Result;
                    response.EnsureSuccessStatusCode();
                }
            };
            channel.BasicConsume(queue: QueueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }


    }
}
