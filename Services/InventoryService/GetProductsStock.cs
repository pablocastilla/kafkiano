using Confluent.Kafka;
using InventoryService.Messages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService
{
    //https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/background-tasks-with-ihostedservice

    public class GetProductsStock : BackgroundService
    {
        IMemoryCache cache;
        InventoryRepository inventoryRepository;


        public GetProductsStock(IMemoryCache memoryCache)
        {
            this.cache = memoryCache;

            inventoryRepository = new InventoryRepository(cache);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CallGetProductsStockToTopic(stoppingToken);
        }

        private async Task CallGetProductsStockToTopic(CancellationToken stoppingToken)
        {

            var config = new ConsumerConfig
            {
                GroupId = "INVENTORYSERVICE",
                BootstrapServers = "127.0.0.1:9092",
                EnableAutoCommit = true
            };

            var switchFromReadingToExecutingCommands = false;

            var topics = new List<string>() { "STOCKBYPRODUCTTABLE", "INVENTORYEVENTS", "ORDEREVENTS" };

            using (var consumer = new Consumer<Ignore, string>(config))
            {
                //first assing to the read model
                consumer.Assign(
                    new List<TopicPartitionOffset>()
                    {
                        new TopicPartitionOffset("STOCKBYPRODUCTTABLE", 0, Offset.Beginning),

                    }
                    );

                consumer.OnError += (_, e)
                        =>
                {
                    Console.WriteLine($"Error: {e.Reason}");
                };

                consumer.OnPartitionEOF += (_, topicPartitionOffset)
                    =>
                {
                    //after reading from stockbyproducttable (read model) we process other events
                    if (!switchFromReadingToExecutingCommands)
                    {
                        consumer.Assign(
                         new List<TopicPartitionOffset>()
                         {
                            new TopicPartitionOffset("INVENTORYEVENTS", 0, Offset.End),
                            new TopicPartitionOffset("ORDEREVENTS", 0, Offset.End),
                         }
                         );

                        switchFromReadingToExecutingCommands = true;
                    }
                };

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        switch (consumeResult.Topic)
                        {
                            //it adds to the stock
                            case "INVENTORYEVENTS":
                                var stockEvent = JsonConvert.DeserializeObject<ProductStockEvent>(consumeResult.Message.Value);
                                inventoryRepository.AddStockToLocalPersistence(stockEvent.ProductName, stockEvent.Quantity);
                               
                                break;

                            //at the begining this stores the stock
                            case "STOCKBYPRODUCTTABLE":
                                var stockInfo = JsonConvert.DeserializeObject<ProductStockInfo>(consumeResult.Message.Value);
                                inventoryRepository.SetStockToLocalPersistence(stockInfo);
                                break;

                            //checks if there is enough stock
                            case "ORDEREVENTS":
                                break;
                        }



                    }
                    catch (Exception e)
                    {
                        //in case of error start again
                        consumer.Assign(
                                new List<TopicPartitionOffset>()
                                {
                                    new TopicPartitionOffset("STOCKBYPRODUCTTABLE", 0, Offset.Beginning),

                               }
                               );

                        switchFromReadingToExecutingCommands = false;
                    }
                }

                consumer.Close();
            }


        }

        

        /*
        static HttpClient client = new HttpClient();
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
        }*/
    }
}
