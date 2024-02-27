using Microsoft.Extensions.Logging;

namespace Messaging.Buffer.Buffer
{
    /// <summary>
    /// Base class for handlers
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public abstract class HandlerBase<TRequest> where TRequest : RequestBase
    {
        protected IMessaging _messaging;
        protected ILogger<HandlerBase<TRequest>> _logger;
        public Type RequestType;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="messaging"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HandlerBase(ILogger<HandlerBase<TRequest>> logger, IMessaging messaging)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messaging = messaging ?? throw new ArgumentNullException(nameof(messaging));
            RequestType = typeof(TRequest);

            // Subscribe this handle to redis.
            _messaging.SubscribeRequestAsync<TRequest>(Handle);
        }

        /// <summary>
        /// Handle request
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="correlationId"></param>
        /// <param name="request"></param>
        public abstract void Handle<TRequest>(string correlationId, TRequest request);


        /// <summary>
        /// Send response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="correlationId"></param>
        /// <param name="response"></param>
        public async Task Respond<TResponse>(string correlationId, TResponse response) where TResponse : ResponseBase
        {
            await _messaging.PublishResponseAsync(correlationId, response);
        }
    }
}
