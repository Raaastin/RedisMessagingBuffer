﻿using System.Collections.Concurrent;
using Messaging.Buffer.Buffer;

namespace Messaging.Buffer
{
    /// <summary>
    /// Messaging interface allowing to publish and subscribe to pub/sub service
    /// </summary>
    public interface IMessaging
    {
        /// <summary>
        /// Event for request received
        /// </summary>
        event EventHandler<ReceivedEventArgs> RequestReceived;


        /// <summary>
        /// Delegate collection for requests. A delegate is added when a request type is subscribed
        /// </summary>
        ConcurrentDictionary<string, Delegate> RequestDelegateCollection { get; set; }

        /// <summary>
        /// Delegate collection for responses. A delegate is added for the lifetime of a buffer
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
        Task SubscribeAnyRequestAsync(EventHandler<ReceivedEventArgs> requestHandler);

        /// <summary>
        /// Subscribe for specific request
        /// </summary>
        /// <param name="requestHandler">Function called on request received</param>
        Task SubscribeRequestAsync<TRequest>(Action<string, TRequest> requestHandler) where TRequest : RequestBase;

        /// <summary>
        /// Unsubscribe for Requests
        /// </summary>
        /// <param name="OnRequest">Function called on request received</param>
        Task UnsubscribeAnyRequestAsync();

        /// <summary>
        /// Unsubscribe specific request
        /// </summary>
        Task UnsubscribeRequestAsync<TRequest>() where TRequest : RequestBase;

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
