using System.Collections.Concurrent;
using Messaging.Buffer.Buffer;

namespace Messaging.Buffer
{
    public interface IMessaging
    {
        /// <summary>
        /// Event for request received
        /// </summary>
        event EventHandler<ReceivedEventArgs> RequestReceived;

        /// <summary>
        /// Delegate collection. A delegate is added for the lifetime of a buffer
        /// </summary>
        ConcurrentDictionary<string, Delegate> ResponseDelegateCollection { get; set; }

        /// <summary>
        /// Publish Request
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="request"></param>
        /// <returns>number of receiver</returns>
        Task<long> PublishRequestAsync(string correlationId, RequestBase request);

        /// <summary>
        /// Publish Response
        /// </summary>
        /// <param name="correlationId"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        Task PublishResponseAsync(string correlationId, ResponseBase response);

        /// <summary>
        /// Subscribe for Requests
        /// </summary>
        /// <param name="OnRequest">Function called on request received</param>
        Task SubscribeAnyRequestAsync(EventHandler<ReceivedEventArgs> OnRequest);

        /// <summary>
        /// Unsubscribe for Requests
        /// </summary>
        /// <param name="OnRequest">Function called on request received</param>
        Task UnsubscribeAnyRequestAsync();

        /// <summary>
        /// Subscribe for response. 
        /// </summary>
        /// <param name="OnRequest">Function called on request received</param>
        Task SubscribeResponseAsync(string correlationId, Action<object, ReceivedEventArgs> responseHandler);

        /// <summary>
        /// Unsubscribe for response. 
        /// </summary>
        /// <param name="OnRequest">Function called on request received</param>
        Task UnsubscribeResponseAsync(string correlationId);

    }
}
