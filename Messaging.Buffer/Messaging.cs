using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Messaging.Buffer.Attributes;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Exceptions;
using Messaging.Buffer.Helpers;
using Messaging.Buffer.Redis;
using Messaging.Buffer.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

[assembly: InternalsVisibleTo("Messaging.Buffer.Test")]
namespace Messaging.Buffer
{

    /// <inheritdoc/>
    public class Messaging : IMessaging
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRedisCollection _redisCollection;
        private readonly ILogger<IMessaging> _logger;

        /// <inheritdoc/>
        public event EventHandler<ReceivedEventArgs> RequestReceived;
        /// <inheritdoc/>
        public ConcurrentDictionary<string, Delegate> RequestDelegateCollection { get; set; }
        /// <inheritdoc/>
        public ConcurrentDictionary<string, Delegate> ResponseDelegateCollection { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public Messaging(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _redisCollection = serviceProvider.GetRequiredService<IRedisCollection>();
            _logger = serviceProvider.GetRequiredService<ILogger<IMessaging>>();
            RequestDelegateCollection = new();
            ResponseDelegateCollection = new();
        }

        // Todo: Test performance on this method
        private static Type ByName(string name)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .Reverse()
                    .Select(assembly => assembly.GetType(name))
                    .FirstOrDefault(t => t != null)
                // Safely delete the following part
                // if you do not want fall back to first partial result
                ??
                AppDomain.CurrentDomain.GetAssemblies()
                    .Reverse()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => t.Name.Contains(name));
        }

        #region Events

        /// <summary>
        /// Method fired when a request is received
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        internal void OnRequest(RedisChannel channel, RedisValue value)
        {
            _logger.LogTrace("Request Received from {Channel}", channel);
            var channelPath = channel.ToString().Split(":");

            if (channelPath.Length != 3)
                throw new Exception($"Unexpected channel format from received message. Channel: {channel}");

            ReceivedEventArgs eventArgs = new ReceivedEventArgs()
            {
                Channel = channel,
                CorrelationId = channelPath[1],
                MessageType = channelPath[2],
                Value = value
            };

            TriggerRequestReceived(eventArgs);
        }

        /// <summary>
        /// Fire delegate or event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void TriggerRequestReceived(ReceivedEventArgs e)
        {
            if (RequestDelegateCollection.TryGetValue(e.MessageType, out Delegate handler))
            {
                // Case: dedicated handler
                try
                {
                    //var assembly = Assembly.GetEntryAssembly();
                    //var name = $"{e.MessageType}, {assembly.FullName}";
                    var type = ByName(e.MessageType);
                    var payload = JsonConvert.DeserializeObject(e.Value, type);
                    handler.DynamicInvoke(e.CorrelationId, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Request could not be handled. Error when calling delegate.");
                }
            }
            else
            {
                // case: generic handler (deprecated soon)
                if (RequestReceived != null)
                {
                    RequestReceived(this, e);
                }
                else
                {
                    _logger.LogError("Could not find any handler associated to the request. Request not handled.");
                }
            }
        }

        /// <summary>
        /// Method fired when a response is received
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        internal void OnResponse(RedisChannel channel, RedisValue value)
        {
            _logger.LogTrace("Response Received from {Channel}", channel);
            var channelPath = channel.ToString().Split(":");

            if (channelPath.Length != 2)
                throw new Exception($"Unexpected channel format from received message. Channel: {channel}");

            ReceivedEventArgs eventArgs = new ReceivedEventArgs()
            {
                Channel = channel,
                CorrelationId = channelPath[1],
                Value = value
            };

            TriggerResponseReceived(eventArgs);
        }

        /// <summary>
        /// Fire delegate
        /// </summary>
        /// <param name="e"></param>
        protected virtual void TriggerResponseReceived(ReceivedEventArgs e)
        {
            if (ResponseDelegateCollection.TryGetValue(e.CorrelationId, out Delegate handler))
            {
                try
                {
                    handler.DynamicInvoke(this, e);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Response could not be handled. Error when calling delegate.");
                }
            }
            else
            {
                _logger.LogError("No Respons delegate found. Could not handle the response.");
            }
        }

        #endregion

        #region Publish Methods

        /// <inheritdoc/>
        public async Task<long> PublishRequestAsync(string correlationId, RequestBase request)
        {
            var requestJson = request.ToJson();
            var type = request.GetType().FullName;
            var channel = $"Request:{correlationId}:{type}";
            long received = 0;

            try
            {
                _logger.LogTrace("Publishing request {Request} - {CorrelationId} to channel {Channel}", type, correlationId, channel);
                received = await _redisCollection.GetSubscriber()?.PublishAsync(RedisChannel.Pattern(channel), requestJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Publish {Request} to channel {Channel}", type, channel);
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
                _logger.LogError(ex, "Could not Publish response {Response} to channel {Channel}", response.GetType().FullName, channel);
            }

            if (received == 0)
                _logger.LogWarning($"Response for of type {response.GetType().FullName} with correlation id {correlationId} lost.");
            if (received > 1)
                _logger.LogWarning($"More than one instance received the Response of type {response.GetType().FullName} with correlation id {correlationId}");
        }

        #endregion

        #region Subscribe/Unsubscribe

        /// <inheritdoc/>
        public async Task SubscribeAnyRequestAsync(EventHandler<ReceivedEventArgs> requestHandler)
        {
            if (RequestReceived is not null)
                throw new SubscriptionException("The subscription for any request is done already.");

            if (RequestDelegateCollection.Any())
                throw new SubscriptionException("Conflicting subscription detected. Cannot perform both request subscription and any request subscription at the same time.");

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
                RequestReceived -= requestHandler;
            }
        }

        /// <inheritdoc/>
        public async Task SubscribeRequestAsync<TRequest>(Action<string, TRequest> requestHandler) where TRequest : RequestBase
        {
            var type = typeof(TRequest).FullName;

            if (RequestReceived is not null)
                throw new SubscriptionException("A subscription for any request is done already. Cannot subscribe for requests independantly.");

            if (RequestDelegateCollection.ContainsKey(type))
                throw new SubscriptionException("This subscription already exists.");

            var channel = $"Request:*:{type}"; // subscribe to TRequest
            try
            {
                if (!RequestDelegateCollection.TryAdd(type, requestHandler))
                    throw new Exception($"Could not add request handler to collection. Subscription canceled.");

                _logger.LogTrace("Subscribing to {Channel}", channel);
                await _redisCollection.SubscribeAsync(RedisChannel.Pattern(channel), OnRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Subscribe to channel {Channel}", channel);
                RequestDelegateCollection.TryRemove(type, out var temp);
            }
        }

        /// <inheritdoc/>
        public async Task SubscribeResponseAsync(string correlationId, Action<object, ReceivedEventArgs> responseHandler)
        {
            if (ResponseDelegateCollection.ContainsKey(correlationId))
                throw new SubscriptionException($"This subscription already exists");

            var channel = $"Response:{correlationId}";
            try
            {
                if (!ResponseDelegateCollection.TryAdd(correlationId, responseHandler))
                    throw new Exception($"Request with correlationId: {correlationId} could not provide OnResponse delegate. Subscription canceled.");

                _logger.LogTrace("Subscribing to {Channel}", channel);
                await _redisCollection.SubscribeAsync(RedisChannel.Pattern(channel), OnResponse); //All request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Subscribe to channel {Channel}", channel);
                ResponseDelegateCollection.TryRemove(correlationId, out var temp);
            }
        }

        private List<string> HandlerSubscribedList = new List<string>();

        /// <inheritdoc/>
        public async Task SubscribeHandlers()
        {
            var handlers = Reflexion.GetHandlerTypes();
            foreach (var handler in handlers)
            {
                if (HandlerSubscribedList.Contains(handler.Name))
                    throw new SubscriptionException($"Dupplicate handler detected. Handler : {handler.Name}.");

                dynamic handlerService = _serviceProvider.GetRequiredService(handler);
                string subscribed = await handlerService.Subscribe();
                HandlerSubscribedList.Add(subscribed);
            }
        }

        /// <inheritdoc/>
        public async Task SubscribeHandler<THandler, TRequest>() where THandler : HandlerBase<TRequest> where TRequest : RequestBase
        {
            dynamic handlerService = _serviceProvider.GetRequiredService(typeof(THandler));
            string subscribed = await handlerService.Subscribe();
            HandlerSubscribedList.Add(subscribed);
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

                RequestReceived = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not Unsubscribe to channel {Channel}", channel);
            }
        }

        /// <inheritdoc/>
        public async Task UnsubscribeRequestAsync<TRequest>() where TRequest : RequestBase
        {
            var type = typeof(TRequest);
            var channel = $"Request:*:{type.FullName}";
            try
            {
                _logger.LogTrace("Unsuscribing channel {Channel}", channel);
                await _redisCollection.UnsubscribeAsync(RedisChannel.Pattern(channel));

                if (!RequestDelegateCollection.TryRemove(type.FullName, out Delegate handler))
                    throw new Exception("Could not remove OnResponse delegate from dictionnary.");
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
                await _redisCollection.UnsubscribeAsync(RedisChannel.Pattern(channel));

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
