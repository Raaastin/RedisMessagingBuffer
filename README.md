# Redis Messaging Buffer
Messaging Buffer is a .Net CORE library used to send request and response throught Redis PubSub service. 

It provides a simple implementation of "buffer" class that allows a single sent request to wait for more than one single response. 

A buffer works by publishing a request to a Redis server. Any subscriber shall handle the request, then publish their response. All responses are published through a temporary specific channel which is subscribed only by the buffer.

## Use case: 
When you have an application that runs on several instances for scaling purpose, and each of these have their own ressouces.

As a user, when requesting the application (ex: through an http request), you don't know which instance is holding your requested ressource. You don't know also which instance will be handling your http request.

Whith no multi instance system, your request will either return your result if your are lucky, or a resource not found response when the request is handled by the 'wrong' instance. 

Using Redis Messaging Buffer, any instance will handle the http request, then ask for other instances for the resource, then respond the http request with the result.

### Use case example: 
User wants the resource A, but unfortunately, its http request is handled by an instance that does not know A.

Pic 1: User requests for the resource A, Instance 1 handles the request and publish a UserWantsA message.
![Request](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/2fa9de0e-6d3b-4f38-8cc5-bae4f36e4df6)

Pic 2: All instances are reading UserWantsA message and respond by publishing their answer.
![request2](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/c2e2e893-ff8f-4ffd-9be9-0fde5e5409ee)

Pic 3: Instance1 is reading all the responses and aggregate all of them into a single response for the user
![Request3](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/79f95499-c56c-4310-b5a7-8025a401f4ac)

MessagingBuffer provides the way to publish and subscribe automatically to the correct channels.

## How to use

### Step 1: Register the service
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/c7b5e947-f0c2-4d1b-ae41-b2d760dfbf00)

Appsettings requirements. The following section must be present in your configuration files

![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/67416edb-4a1e-4a33-8f61-54ea17c6597f)

MessagingBuffer can handle multiple redis connexion string. (note: When more than one Redis server is used, subscriptions are all duplicated, and messages are published randomly through an available Redis Server)

### Step 3: Create your classes Buffer, Request and Response.
- Your Buffer class must implement RequestBufferBase. 
- Your Request class must implement RequestBase.
- Your Response class must implement ResponseBase.
- **Mandatory**: Implement Aggregate method: This method shall read ResponseCollection from buffer base and aggregate results in order to return a single Response object.

#### Example: TotalCount request
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/5c54811b-5187-4d6c-9cf9-0d7372e89dc0)
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/dfb268cf-df87-4740-be2f-dd9aae4141bc)

### Step 4: Register Buffer
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/a9e59ebd-931e-4b8c-b558-8e827b0346f0)

### Step 5: Subscribe to some redis Channel and register message handlers
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/5c28497e-1651-4aa0-87ea-f7f06f38765d)
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/6fa79167-be8a-43d4-8bcf-72323896f7fe)

### Step 6: Get a Buffer through the service provider, then send the request.
![image](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/ca200b14-bd24-46e7-880d-67a84ed9e842)

In this example, response contains the result of the Aggregate method from the buffer.

## Pub/Sub Details
- When using Messaging.Buffer, all of your application instances will subscribe to pub/sub channel "Request:\*:\*".
- (optional) it's possible to perform several independant subsciption instead (ex: "Request:\*:RequestA", "Request:\*:\RequestB", etc)
- Each time a buffer is created, on sending the request, the buffer will subscribe to channel "Response:\[your.buffer.unique.id\]" in order to handle all incoming responses
- Each instance of the application, on request received, shall handle the request and respond to channel "Response:\[your.buffer.unique.id\]"
- Once all responses are received, or after a time out delay, the channel "Response:\[your.buffer.unique.id\]" is unsubscribed by the buffer, the responses are aggregated and returned.
