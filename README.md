# Install kafka docker image
https://docs.confluent.io/current/installation/docker/docs/installation/single-node-client.html

#### put kafka in host file
127.0.0.1		kafka

#### create docker network and run images

docker network create confluent

1. zookeeper
   - docker run -d --net=confluent --name=zookeeper -e ZOOKEEPER_CLIENT_PORT=2181 confluentinc/cp-zookeeper:5.0.1
2. Kafka
   - docker run -d --net=confluent --name=kafka -e KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181 -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka:9092 -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 -p 9092:9092 confluentinc/cp-kafka:5.0.1
3. Schema registry
   - docker run -d --net=confluent --name=schema-registry -e SCHEMA_REGISTRY_KAFKASTORE_CONNECTION_URL=zookeeper:2181 -e SCHEMA_REGISTRY_HOST_NAME=schema-registry -e SCHEMA_REGISTRY_LISTENERS=http://0.0.0.0:8081 confluentinc/cp-schema-registry:5.0.1
4. REST proxy
   - docker run -d --net=confluent --name=kafka-rest -e KAFKA_REST_ZOOKEEPER_CONNECT=zookeeper:2181 -e KAFKA_REST_LISTENERS=http://0.0.0.0:8082 -e KAFKA_REST_SCHEMA_REGISTRY_URL=http://schema-registry:8081 -e KAFKA_REST_HOST_NAME=kafka-rest confluentinc/cp-kafka-rest:5.0.1
5. KSQL
   - docker run -d --net=confluent --name=ksql-server -e KSQL_BOOTSTRAP_SERVERS=kafka:9092 -e KSQL_LISTENERS=http://0.0.0.0:8088 -e KSQL_KSQL_SCHEMA_REGISTRY_URL=http://schema-registry:8081 -p 8088:8088 confluentinc/cp-ksql-server:5.0.1
6. KSQL CLI
   - docker run -it --net=confluent confluentinc/cp-ksql-cli http://ksql-server:8088

#### stop and remove containers
docker stop $(docker ps -a -q) 
docker rm $(docker ps -a -q)


# Example in .net

https://github.com/confluentinc/kafka-streams-examples/tree/3.3.1-post/src/main/java/io/confluent/examples/streams/microservices