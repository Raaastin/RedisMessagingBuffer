# Redis Messaging Buffer
Messaging Buffer is a .Net CORE library used to send request and response throught Redis PubSub service. 

It provides a simple implementation of "buffer" class that allows a single sent request to wait for more than one single response. 

A buffer works by publishing a request to a Redis server. Any subscriber shall handle the request, then publish their response. All responses are published through a temporary specific channel which is subscribed only by the buffer.

## Use case: 
When you have an application that runs on several instances for scaling purpose, and each of these have their own ressouces
As a user, when requesting the application (ex: through an http request), you don't know which instance is holding your requested ressource. You don't know also which instance will be handling your http request.
Whith no multi instance system, your request will either return your result if your are lucky, or a resource not found response when the request is handled by the 'wrong' instance. 
Using Redis Messaging Buffer, any instance will handle the http request, then ask for other instances for the resource, then respond the http request with the result.

### Use case example: 
User wants the resource A, but unfortunately, its http request is handled by an instance that does not know A.

Pic 1: User requests for the resource A, Instance 1 handles the request and publish a UserWantsA message.
![Request](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/ef3cfb78-20a2-4b3d-a2b6-e80f5046e7cc)

Pic 2: All instances are reading UserWantsA message and respond by publishing their answer.
![request2](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/e7bd51d9-7570-45b6-9543-164225988dc8)

Pic 3: Instance1 is reading all the responses and aggregate all of them into a single response for the user
![Request3](https://github.com/Raaastin/RedisMessagingBuffer/assets/160628718/54822aad-5b2b-49be-bf3c-de6fe0f5b706)

MessagingBuffer provides the way to publish and subscribe automatically to the correct channels.

## How to use

todo readme: 
- examples
- limitations
- How to use

todo project: 
- test project
- GetResourceA example in the testApp
- fire and forget case
- better error handling

