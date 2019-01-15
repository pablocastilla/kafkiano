# Services with Kafka and dotnet (January 2019)
### Intention
Hi! 

This is a repository with a failed experiment. I just wanted to implement services using kafka and dotnet like it is shown in this article for the JVM:
https://www.confluent.io/blog/building-a-microservices-ecosystem-with-kafka-streams-and-ksql/ 

What I love from it is that it seems you can have kafka for messaging and storing data all in a exactly once fashion. I see it like the holy grail and if that is possible kafka can be a great election for event publishing but also for implementing transactional logic (C in the CQRS world). 

### Why is it a failure?
If you see this java code the trick is in the transform function
https://github.com/confluentinc/kafka-streams-examples/blob/3.3.1-post/src/main/java/io/confluent/examples/streams/microservices/InventoryService.java 

It access a storage in the kafka node and execute the logic based on the information from it in a exactly once mode. The problem is I wasn't able to implement it in dotnet: 

* Although you can order the logic per entity and execute the entity logic one by one you don't have access to the kafka node rockdb that can participate in the kafka transaction.
* In KSQL you can have a lot of queries that are executed in parallel, this is great for streaming calculus or creating read models, but I don't see KSQL can be useful with logic that just have a certain complexity. 

### What can you do with Kafka and dotnet?
* You can use it as a log and subscribe to it, you can read a message in an "at least once" mode, making the usual tricks for having idempotence. This is huge. But I don't see you can use it as a transport+transactional storage.


### What would we need to have transport+storage?
* Transactions in the dotnet driver (they are working on it)
* Access to the keyvalue store (rockdb) in the transaction (I think this is not in the roadmap).

### What have I learnt in this failure?
* KSQL
* More kafka (I love the ordering by key feature!)
