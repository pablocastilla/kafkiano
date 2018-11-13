using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OrdersService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };


            Run_Consume(new List<string>() {"OrdersCommands"}, cts.Token);
        }


        public static void Run_Consume(List<string> topics, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "127.0.0.1:9092",
                GroupId = "csharp-consumer",
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };

            const int commitPeriod = 5;

            using (var consumer = new Consumer<Ignore, string>(config))
            {
                // Note: All event handlers are called on the main .Consume thread.

                // Raised when the consumer has been notified of a new assignment set.
                // You can use this event to perform actions such as retrieving offsets
                // from an external source / manually setting start offsets using
                // the Assign method. You can even call Assign with a different set of
                // partitions than those in the assignment. If you do not call Assign
                // in a handler of this event, the consumer will be automatically
                // assigned to the partitions of the assignment set and consumption
                // will start from last committed offsets or in accordance with
                // the auto.offset.reset configuration parameter for partitions where
                // there is no committed offset.
                consumer.OnPartitionsAssigned += (_, partitions)
                    => Console.WriteLine($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {consumer.MemberId}");

                // Raised when the consumer's current assignment set has been revoked.
                consumer.OnPartitionsRevoked += (_, partitions)
                    => Console.WriteLine($"Revoked partitions: [{string.Join(", ", partitions)}]");

                consumer.OnPartitionEOF += (_, tpo)
                    => Console.WriteLine($"Reached end of topic {tpo.Topic} partition {tpo.Partition}, next message will be at offset {tpo.Offset}");

                consumer.OnError += (_, e)
                    => Console.WriteLine($"Error: {e.Reason}");

                consumer.OnStatistics += (_, json)
                    => Console.WriteLine($"Statistics: {json}");

                consumer.Subscribe(topics);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Console.WriteLine($"Topic: {consumeResult.Topic} Partition: {consumeResult.Partition} Offset: {consumeResult.Offset} {consumeResult.Value}");

                        if (consumeResult.Offset % commitPeriod == 0)
                        {
                            // The Commit method sends a "commit offsets" request to the Kafka
                            // cluster and synchronously waits for the response. This is very
                            // slow compared to the rate at which the consumer is capable of
                            // consuming messages. A high performance application will typically
                            // commit offsets relatively infrequently and be designed handle
                            // duplicate messages in the event of failure.
                            var committedOffsets = consumer.Commit(consumeResult);
                            Console.WriteLine($"Committed offset: {committedOffsets}");
                        }
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error}");
                    }
                }

                consumer.Close();
            }
        }

    }
}
