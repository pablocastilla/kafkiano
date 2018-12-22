﻿using Confluent.Kafka;
using Constants;
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

    public class EventLogic : BackgroundService
    {
        IMemoryCache cache;
        InventoryRepository inventoryRepository;
                       

        public EventLogic(IMemoryCache memoryCache)
        {
            this.cache = memoryCache;

            inventoryRepository = new InventoryRepository(cache);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await EventLogicByTopic(stoppingToken);
        }

        private async Task EventLogicByTopic(CancellationToken stoppingToken)
        {

            var config = new ConsumerConfig
            {
                GroupId = "INVENTORYSERVICE",
                BootstrapServers = "127.0.0.1:9092",
                EnableAutoCommit = true
            };

           

            var topics = new List<string>() { TOPICS.INVENTORYEVENTS, TOPICS.ORDERSEVENTS };

            using (var consumer = new Consumer<Ignore, string>(config))
            {


                consumer.Subscribe(topics);

                consumer.OnError += (_, e)
                        =>
                {
                    
                };

                consumer.OnPartitionsRevoked += (_, topicPartitionOffset)
                     =>
                {
                   
                };

                consumer.OnPartitionEOF += (_, topicPartitionOffset)
                    =>
                {

                };

                while (!stoppingToken.IsCancellationRequested)
                {
 
                        var consumeResult = consumer.Consume(stoppingToken);

                        switch (consumeResult.Topic)
                        {

                            //it adds to the stock
                            case TOPICS.INVENTORYEVENTS:
                                var stockEvent = JsonConvert.DeserializeObject<ProductStockEvent>(consumeResult.Message.Value);

                                switch (stockEvent.Action)
                                {
                                    case StockAction.Add:
                                        var stock = inventoryRepository.AddStockToPersistence(stockEvent.ProductName, stockEvent.Quantity);
                                        await PublishEvent(TOPICS.STOCKBYPRODUCTTOPIC, stockEvent.ProductName, new ProductStockInfo() { ProductName = stockEvent.ProductName, Stock = stock });
                                        break;

                                    case StockAction.Remove:
                                        stock = inventoryRepository.AddStockToPersistence(stockEvent.ProductName, stockEvent.Quantity*-1);
                                        await PublishEvent(TOPICS.STOCKBYPRODUCTTOPIC, stockEvent.ProductName, new ProductStockInfo() { ProductName = stockEvent.ProductName, Stock = stock });
                                        break;

                                    case StockAction.Validate:
                                        stock = inventoryRepository.GetStockFromPersistence(stockEvent.ProductName);
                                        if(stock>0)
                                        {
                                            //publish "order validated event" and remove one
                                            stock = inventoryRepository.AddStockToPersistence(stockEvent.ProductName, stockEvent.Quantity * -1);
                                            await PublishEvent(TOPICS.INVENTORYVALIDATIONSEVENTS, stockEvent.OrderId, new ProductStockValidated() {Ok=true,OrderId=stockEvent.OrderId,ProductName=stockEvent.ProductName });

                                        }
                                        else
                                        {
                                            //publish "order invalid event"
                                            await PublishEvent(TOPICS.INVENTORYVALIDATIONSEVENTS, stockEvent.OrderId, new ProductStockValidated() { Ok = false, OrderId = stockEvent.OrderId, ProductName = stockEvent.ProductName });
                                        }
                                        
                                        break;
                                }
                               
                                break;


                            //checks if there is enough stock
                            case TOPICS.ORDERSEVENTS:
                                break;
                        }

                }

                consumer.Close();
            }


        }

        private async Task PublishEvent<TKey,TMessage>(string topic, TKey key, TMessage message)
        {

            var config = new ProducerConfig { BootstrapServers = "127.0.0.1:9092" };

            using (var producer = new Producer<TKey, string>(config))
            {
                var deliveryReport = await producer.ProduceAsync(topic, new Message<TKey, string>
                {
                    Key = key,
                    Value = JsonConvert.SerializeObject(message)
                });


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
