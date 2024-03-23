using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Buffer.Service
{
    /// <summary>
    /// Configurator class for Messaging service
    /// </summary>
    public class MessagingConfigurator
    {
        private IServiceCollection _services;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="services"></param>
        public MessagingConfigurator(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Register a buffer, request, response
        /// </summary>
        /// <typeparam name="TBuffer"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public MessagingConfigurator AddBuffer<TBuffer, TRequest, TResponse>()
            where TBuffer : RequestBufferBase<TRequest, TResponse>
            where TRequest : RequestBase
            where TResponse : ResponseBase
        {
            _services.AddTransient<TBuffer>();
            _services.AddTransient<TRequest>();
            return this;
        }

        /// <summary>
        /// Register a buffer, request, response, handler
        /// </summary>
        /// <typeparam name="TBuffer"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        public MessagingConfigurator AddBuffer<TBuffer, TRequest, TResponse, THandler>()
            where TBuffer : RequestBufferBase<TRequest, TResponse>
            where TRequest : RequestBase
            where TResponse : ResponseBase
            where THandler : HandlerBase<TRequest>
        {
            _services.AddTransient<TBuffer>();
            _services.AddTransient<TRequest>();
            _services.AddSingleton<THandler>();
            return this;
        }

        /// <summary>
        /// Automatically search and register requests, responses, buffers and handlers.
        /// </summary>
        /// <returns></returns>
        public MessagingConfigurator AutomaticConfiguration()
        {
            RegisterRequests();
            RegisterRequestBuffers();
            RegisterHandlers();

            return this;
        }


        /// <summary>
        /// Register a handler for a request
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        public MessagingConfigurator RegisterHandler<TRequest, THandler>() where THandler : HandlerBase<TRequest> where TRequest : RequestBase
        {
            _services.AddSingleton<THandler>();
            return this;
        }

        /// <summary>
        /// Register all handlers that reference HandlerAttribute
        /// </summary>
        /// <returns></returns>
        public MessagingConfigurator RegisterHandlers()
        {
            var handlers = Reflexion.GetHandlerTypes();
            foreach (var handler in handlers)
            {
                _services.AddSingleton(handler);
            }
            return this;
        }

        /// <summary>
        /// Register all requests that reference RequestAttribute
        /// </summary>
        /// <returns></returns>
        public MessagingConfigurator RegisterRequests()
        {
            var requests = Reflexion.GetRequestTypes();
            foreach (var request in requests)
            {
                _services.AddTransient(request);
            }
            return this;
        }

        /// <summary>
        /// Register all request buffers that reference RequestBufferAttribute
        /// </summary>
        /// <returns></returns>
        public MessagingConfigurator RegisterRequestBuffers()
        {
            var buffers = Reflexion.GetRequestBufferTypes();
            foreach (var buffer in buffers)
            {
                _services.AddTransient(buffer);
            }
            return this;
        }
    }
}
