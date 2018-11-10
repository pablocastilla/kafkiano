# Install kafka docker image
https://docs.confluent.io/current/installation/docker/docs/installation/single-node-client.html

## put kafka in host file
127.0.0.1		kafka

## create docker network and run images

docker network create confluent

zookeeper
docker run -d --net=confluent --name=zookeeper -e ZOOKEEPER_CLIENT_PORT=2181 confluentinc/cp-zookeeper:5.0.1
Kafka
docker run -d --net=confluent --name=kafka -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka:9092 -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 confluentinc/cp-kafka:5.0.1
Schema registry
docker run -d --net=confluent --name=schema-registry -e SCHEMA_REGISTRY_KAFKASTORE_CONNECTION_URL=zookeeper:2181 -e SCHEMA_REGISTRY_HOST_NAME=schema-registry -e SCHEMA_REGISTRY_LISTENERS=http://0.0.0.0:8081 confluentinc/cp-schema-registry:5.0.1
REST proxy
docker run -d --net=confluent --name=kafka-rest -e KAFKA_REST_ZOOKEEPER_CONNECT=zookeeper:2181 -e KAFKA_REST_LISTENERS=http://0.0.0.0:8082 -e KAFKA_REST_SCHEMA_REGISTRY_URL=http://schema-registry:8081 -e KAFKA_REST_HOST_NAME=kafka-rest confluentinc/cp-kafka-rest:5.0.1
KSQL
docker run -d --net=confluent --name=ksql-server -e KSQL_BOOTSTRAP_SERVERS=kafka:9092 -e KSQL_LISTENERS=http://0.0.0.0:8088 -e KSQL_KSQL_SCHEMA_REGISTRY_URL=http://schema-registry:8081 confluentinc/cp-ksql-server:5.0.1
KSQL CLI
docker run -it confluentinc/cp-ksql-cli http://127.0.0.1:8088


### create topics

create topic
docker run --net=confluent --rm confluentinc/cp-kafka:5.0.1 kafka-topics --create --topic OrdersCommands --partitions 1 --replication-factor 1 --if-not-exists --zookeeper zookeeper:2181
docker run --net=confluent --rm confluentinc/cp-kafka:5.0.1 bash -c "seq 42 | kafka-console-producer --request-required-acks 1 --broker-list kafka:9092 --topic OrdersCommands && echo 'Produced 42 messages.'"




# Example in .net

https://github.com/confluentinc/kafka-streams-examples/tree/3.3.1-post/src/main/java/io/confluent/examples/streams/microservices