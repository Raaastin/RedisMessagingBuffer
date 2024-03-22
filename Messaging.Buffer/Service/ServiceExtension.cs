using Messaging.Buffer.Attributes;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.Helpers;
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
        public static IServiceCollection AddMessagingBuffer(this IServiceCollection services, IConfiguration configuration, string configSection, Action<MessagingConfigurator> configurator)
        {
            services.Configure<RedisOptions>(configuration.GetSection(configSection));

            services.AddSingleton<IRedisCollection, RedisCollection>();
            services.AddSingleton<IMessaging, Messaging>();

            configurator.Invoke(new MessagingConfigurator(services));

            return services;
        }

        #endregion Public Methods
    }
}