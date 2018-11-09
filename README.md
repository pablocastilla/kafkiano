# Install kafka docker image
https://docs.confluent.io/current/installation/docker/docs/installation/single-node-client.html

## put kafka in host file
127.0.0.1		kafka

## create docker network and run images

docker network create confluent
docker run -d --net=confluent --name=zookeeper -e ZOOKEEPER_CLIENT_PORT=2181 confluentinc/cp-zookeeper:5.0.1
docker run -d --net=confluent --name=kafka -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka:9092 -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 confluentinc/cp-kafka:5.0.1

### create topics

create topic
docker run --net=confluent --rm confluentinc/cp-kafka:5.0.1 kafka-topics --create --topic OrdersCommands --partitions 1 --replication-factor 1 --if-not-exists --zookeeper zookeeper:2181
docker run --net=confluent --rm confluentinc/cp-kafka:5.0.1 bash -c "seq 42 | kafka-console-producer --request-required-acks 1 --broker-list kafka:9092 --topic OrdersCommands && echo 'Produced 42 messages.'"


# Example in .net

https://github.com/confluentinc/kafka-streams-examples/tree/3.3.1-post/src/main/java/io/confluent/examples/streams/microservices