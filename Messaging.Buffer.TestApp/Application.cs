﻿using System.Reflection;
using Messaging.Buffer.TestApp.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Messaging.Buffer.TestApp
{
    public class Application
    {
        private IMessaging _messaging;
        private IServiceProvider _serviceProvider;
        private ILogger<Application> _logger;

        private int personalCount = 5;

        public Application(IMessaging messaging, ILogger<Application> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _messaging = messaging;
            _serviceProvider = serviceProvider;

            // (deprecated)
            //_messaging.SubscribeAnyRequestAsync(OnRequest);

            _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
            _messaging.SubscribeRequestAsync<TotalCountRequest>(OnTotalCountRequestReceived);
        }

        /// <summary>
        /// Hello world example: No input in request
        /// </summary>
        public async Task RunHelloWorld()
        {
            _logger.LogTrace("Performing HelloWorld process");

            var buffer = _serviceProvider.GetRequiredService<HelloWorldRequestBuffer>();
            var response = await buffer.SendRequestAsync();

            _logger.LogTrace($"Response from several apps:\r\n{response.InstanceResponse}");
        }

        /// <summary>
        /// Total Count example: input in request
        /// </summary>
        public async Task RunTotalCount()
        {
            _logger.LogTrace("Performing TotalCount process");

            var buffer = _serviceProvider.GetRequiredService<TotalCountRequestBuffer>();
            buffer.Request.InitialCount = 100;
            var response = await buffer.SendRequestAsync();

            _logger.LogTrace($"Total count among all the apps: {response.Count}");
        }

        private async void OnRequest(object sender, ReceivedEventArgs e)
        {
            switch (e.MessageType)
            {
                case "HelloWorldRequest":
                    dynamic request = JsonConvert.DeserializeObject<HelloWorldRequest>(e.Value);
                    await _messaging.PublishResponseAsync(e.CorrelationId, new HelloWorldResponse(e.CorrelationId)
                    {
                        InstanceResponse = $"Hello from {Environment.UserName}"
                    });
                    break;
                case "TotalCountRequest":
                    request = JsonConvert.DeserializeObject<TotalCountRequest>(e.Value);
                    await _messaging.PublishResponseAsync(e.CorrelationId, new TotalCountResponse(e.CorrelationId, personalCount));
                    break;
                default:
                    throw new NotImplementedException("No Deserialization possible with this request");
            }
        }

        private async void OnHelloWorldRequestReceived(string correlationId, HelloWorldRequest request)
        {
            _logger.LogTrace($"Method: {MethodBase.GetCurrentMethod().Name}");
            await _messaging.PublishResponseAsync(correlationId, new HelloWorldResponse(correlationId)
            {
                InstanceResponse = $"Hello from {Environment.UserName}"
            });
        }

        private async void OnTotalCountRequestReceived(string correlationId, TotalCountRequest request)
        {
            _logger.LogTrace($"Method: {MethodBase.GetCurrentMethod().Name}");
            await _messaging.PublishResponseAsync(correlationId, new TotalCountResponse(correlationId, personalCount));
        }
    }
}
