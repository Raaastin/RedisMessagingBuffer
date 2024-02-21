using System.Collections.Concurrent;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Redis;
using Messaging.Buffer.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Messaging.Buffer
{

    public class Messaging : IMessaging
    {
        private readonly IRedisCollection _redisCollection;
        private readonly ILogger<IMessaging> _logger;

        public event EventHandler<ReceivedEventArgs> RequestReceived;
        public ConcurrentDictionary<string, Delegate> ResponseDelegateCollection { get; set; }

        public Messaging(ILogger<IMessaging> logger, IRedisCollection redisCollection)
        {
            _redisCollection = redisCollection;
            _logger = logger;
            ResponseDelegateCollection = new();
        }

        #region Events

        private void OnRequest(RedisChannel channel, RedisValue value)
        {
            _logger.LogTrace("Request Received from {Channel}", channel);
            var channelPath = channel.ToString().Split(":");

            ReceivedEventArgs eventArgs = new ReceivedEventArgs()
            {
                Channel = channel,
                CorrelationId = channelPath[1],
                MessageType = channelPath[2],
                Value = value
            };

            TriggerRequestReceived(eventArgs);
        }

        protected virtual void TriggerRequestReceived(ReceivedEventArgs e)
        {
            EventHandler<ReceivedEventArgs> handler = RequestReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnResponse(RedisChannel channel, RedisValue value)
        {
            _logger.LogTrace("Response Received from {Channel}", channel);
            var channelPath = channel.ToString().Split(":");

            ReceivedEventArgs eventArgs = new ReceivedEventArgs()
            {
                Channel = channel,
                CorrelationId = channelPath[1],
                Value = value
            };

            TriggerResponseReceived(eventArgs);
        }

        protected virtual void TriggerResponseReceived(ReceivedEventArgs e)
        {
            if (ResponseDelegateCollection.TryGetValue(e.CorrelationId, out Delegate handler))
            {
                try
                {
                    handler.DynamicInvoke(this, e);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Response could not be handled. Error when calling delegate.");
                }
            }
        }

        #endregion

        #region Publish Methods

        /// <inheritdoc/>
        public async Task<long> PublishRequestAsync(string correlationId, RequestBase request)
        {
            var requestJson = request.ToJson();
            var type = request.GetType().Name;
            var channel = $"Request:{correlationId}:{type}";
            long received = 0;

            try
            {
                _logger.LogTrace("Publishing request {Request} - {CorrelationId} to channel {Channel}", type, correlationId, channel);
                received = await _redisCollection.GetSubscriber()?.PublishAsync(RedisChannel.Pattern(channel), requestJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Publish {Request} to channel {Channel}", request.GetType().Name, channel);
            }

            if (received == 0)
                _logger.LogWarning($"Request of type {type} {correlationId} lost.");

            return received;
        }

        /// <inheritdoc/>
        public async Task PublishResponseAsync(string correlationId, ResponseBase response)
        {
            var responseJson = response.ToJson();
            var channel = $"Response:{correlationId}";
            long received = 0;

            try
            {
                _logger.LogTrace("Publishing response with id {CorrelationId} to channel {Channel}", correlationId, channel);
                received = await _redisCollection.GetSubscriber().PublishAsync(RedisChannel.Pattern(channel), responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Publish response {Response} to channel {Channel}", response.GetType().Name, channel);
            }

            if (received == 0)
                _logger.LogWarning($"Response for of type {response.GetType().Name} with correlation id {correlationId} lost.");
            if (received > 1)
                _logger.LogWarning($"More than one instance received the Response of type {response.GetType().Name} with correlation id {correlationId}");
        }

        #endregion

        #region Subscribe/Unsubscribe

        /// <inheritdoc/>
        public async Task SubscribeAnyRequestAsync(EventHandler<ReceivedEventArgs> requestHandler)
        {
            var channel = $"Request:*:*"; // subscribe to all possible request
            try
            {
                RequestReceived += requestHandler;

                _logger.LogTrace("Subscribing to {Channel}", channel);
                await _redisCollection.SubscribeAsync(RedisChannel.Pattern(channel), OnRequest); //All request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Subscribe to channel {Channel}", channel);
            }
        }

        /// <inheritdoc/>
        public async Task SubscribeResponseAsync(string correlationId, Action<object, ReceivedEventArgs> responseHandler)
        {
            var channel = $"Response:{correlationId}";
            try
            {
                if (!ResponseDelegateCollection.TryAdd(correlationId, responseHandler))
                    throw new Exception($"Request with correlationId: {correlationId} could not provide OnResponse delegate. Request canceled.");

                _logger.LogTrace("Subscribing to {Channel}", channel);
                await _redisCollection.SubscribeAsync(RedisChannel.Pattern(channel), OnResponse); //All request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Subscribe to channel {Channel}", channel);
            }
        }

        #endregion

        #region Unsubscribe

        /// <inheritdoc/>
        public async Task UnsubscribeAnyRequestAsync()
        {
            var channel = $"Request:*:*";
            try
            {
                _logger.LogTrace("Unsuscribing channel {Channel}", channel);
                await _redisCollection.UnsubscribeAsync(RedisChannel.Pattern(channel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Unsubscribe to channel {Channel}", channel);
            }
        }

        /// <inheritdoc/>
        public async Task UnsubscribeResponseAsync(string correlationId)
        {
            var channel = $"Response:{correlationId}";
            try
            {
                _logger.LogTrace("Unsuscribing channel {Channel}", channel);
                await _redisCollection.UnsubscribeAsync(RedisChannel.Pattern(channel)); //All request

                if (!ResponseDelegateCollection.TryRemove(correlationId, out Delegate handler))
                    throw new Exception("Could not remove OnResponse delegate from dictionnary.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Unsubscribe to channel {Channel}", channel);
            }
        }

        #endregion
    }
}
