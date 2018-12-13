using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebUI
{
    //https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/background-tasks-with-ihostedservice

    public interface IGetProductStock { }

    public class GetProductStock : IGetProductStock
    {
        static HttpClient client = new HttpClient();

        Task executeCall;

        public GetProductStock()
        {
            executeCall = Task.Run(
                () => CallGetProductsStockToTopic()
                );


        }

        private void CallGetProductsStockToTopic()
        {

            var config = new ConsumerConfig
            {
                // the group.id property must be specified when creating a consumer, even 
                // if you do not intend to use any consumer group functionality.
                GroupId = "UI",
                BootstrapServers = "127.0.0.1:9092",
                // partition offsets can be committed to a group even by consumers not
                // subscribed to the group. in this example, auto commit is disabled
                // to prevent this from occuring.
                EnableAutoCommit = true
            };

            var topics = new List<string>() { "STOCKBYPRODUCTTABLE" };

            using (var consumer = new Consumer<Ignore, string>(config))
            {
                consumer.Assign(topics.Select(topic => new TopicPartitionOffset(topic, 0, Offset.Beginning)).ToList());

                consumer.OnError += (_, e)
                        => Console.WriteLine($"Error: {e.Reason}");

                consumer.OnPartitionEOF += (_, topicPartitionOffset)
                    => Console.WriteLine($"End of partition: {topicPartitionOffset}");

                while (true)
                {
                    try
                    {
                        var consumeResult = consumer.Consume();
                        Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: ${consumeResult.Message}");
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error}");
                    }
                }

                consumer.Close();
            }


        }

        private void CallGetProductStockRESTToSQL()
        {
            var url = "http://localhost:8088/query";


            client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/vnd.ksql.v1+json");
            //request.Headers.Add("Content-Type", "application/vnd.ksql.v1+json");

            request.Content = new StringContent(
                        "{\"ksql\": \"SELECT * from StockByProductTable;\",\"streamsProperties\": {\"ksql.streams.auto.offset.reset\": \"earliest\"}}",
                        Encoding.UTF8, "application/vnd.ksql.v1+json");

            using (var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                using (var body = response.Result.Content.ReadAsStreamAsync().Result)
                using (var reader = new StreamReader(body))
                {
                    while (!reader.EndOfStream)
                    {
                        var message = reader.ReadToEndAsync().Result;

                    }
                }
            }
        }
    }
}
