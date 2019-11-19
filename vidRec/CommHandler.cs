using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json.Linq;

namespace vidRec
{
    class CommHandler
    {

        private IConnection connection = null;
        private IModel SendChannel = null;
        private IModel ReceiveChannel = null;

        public CommHandler(string ip)
        {
            var factory = new ConnectionFactory();
            factory.UserName = "audio";
            factory.Password = "audio";
            factory.VirtualHost = "/";
            factory.Port = 5672;
            factory.HostName = ip;
            factory.RequestedHeartbeat = 10;
            factory.AutomaticRecoveryEnabled = true;
            connection = factory.CreateConnection();
        }

        public void send(string exchange, string routing, string content)
        {
            try
            {
                if ( SendChannel==null || SendChannel.IsClosed)
                {
                    SendChannel = connection.CreateModel();
                    if (exchange.Contains("fanout"))
                        SendChannel.ExchangeDeclare(exchange: exchange, type: "fanout", durable: true);
                    else if (exchange.Contains("direct"))
                        SendChannel.ExchangeDeclare(exchange: exchange, type: "direct", durable: true);
                    else if (exchange.Contains("topic"))
                        SendChannel.ExchangeDeclare(exchange: exchange, type: "topic", durable: true);
                    else
                        exchange = "";
                }
                var body = Encoding.UTF8.GetBytes(content);
                SendChannel.BasicPublish(exchange: exchange,
                                            routingKey: routing,
                                            basicProperties: null,
                                            body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CommHandler.send(): " + ex.Message);
            }
        }


        public void listen(string exchange, string routing, Action<string> callback)
        {
            if (ReceiveChannel == null || ReceiveChannel.IsClosed)
            {
                ReceiveChannel = connection.CreateModel();
                if (exchange.Contains("fanout"))
                    ReceiveChannel.ExchangeDeclare(exchange: exchange, type: "fanout", durable: true);
                else if (exchange.Contains("direct"))
                    ReceiveChannel.ExchangeDeclare(exchange: exchange, type: "direct", durable: true);
                else if (exchange.Contains("topic"))
                    ReceiveChannel.ExchangeDeclare(exchange: exchange, type: "topic", durable: true);
                else
                    exchange = "";
            }
            // queue
            var queueName = ReceiveChannel.QueueDeclare().QueueName;
            // binding
            if ("" == exchange)
                Console.WriteLine("[*] The queue binds to the default exchange");
            else
                ReceiveChannel.QueueBind(queue: queueName,
                                         exchange: exchange,
                                         routingKey: routing);
                Console.WriteLine(String.Format("[*] Listening to <{0}> on <{1}>", routing, queueName));
                var consumer = new EventingBasicConsumer(ReceiveChannel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string message = Encoding.UTF8.GetString(body);
                    callback(message);
                };
                ReceiveChannel.BasicConsume(queue: queueName,
                                            autoAck: true,
                                            consumer: consumer);
        }


        public void close()
        {
            try {
                if (SendChannel != null && SendChannel.IsOpen)
                    SendChannel.Close();
                if (ReceiveChannel != null && ReceiveChannel.IsOpen)
                    ReceiveChannel.Close();
                connection.Close();
                connection = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CommHandler.close(): " + ex.Message);
            }
        }

    }
}
