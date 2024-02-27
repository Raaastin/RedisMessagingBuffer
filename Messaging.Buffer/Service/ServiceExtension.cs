using Messaging.Buffer.Buffer;
using Messaging.Buffer.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Buffer.Service
{
    public static class ServiceExtension
    {
        #region Public Methods

        /// <summary>
        /// Add the messaging buffer base
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="configSection"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessagingBuffer(this IServiceCollection services, IConfiguration configuration, string configSection)
        {
            services.Configure<RedisOptions>(configuration.GetSection(configSection));

            services.AddSingleton<IRedisCollection, RedisCollection>();
            services.AddSingleton<IMessaging, Messaging>();

            return services;
        }

        /// <summary>
        /// Register a buffer, request, response
        /// </summary>
        /// <typeparam name="TBuffer"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBuffer<TBuffer, TRequest, TResponse>(this IServiceCollection services)
            where TBuffer : RequestBufferBase<TRequest, TResponse>
            where TRequest : RequestBase
            where TResponse : ResponseBase
        {
            services.AddTransient<TBuffer>();
            services.AddTransient<TRequest>();
            return services;
        }

        /// <summary>
        /// Register a handler for a request
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterHandler<TRequest, THandler>(this IServiceCollection services) where THandler : HandlerBase<TRequest> where TRequest : RequestBase
        {
            services.AddSingleton<THandler>();
            return services;
        }

        #endregion Public Methods
    }
}