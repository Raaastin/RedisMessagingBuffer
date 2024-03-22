using System;
using System.Reflection;
using Messaging.Buffer.Exceptions;
using Messaging.Buffer.TestApp.Handlers;
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

        public List<string> ResourceList { get; set; } = new();

        public Application(IMessaging messaging, ILogger<Application> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _messaging = messaging;
            _serviceProvider = serviceProvider;

            //Generate dummy resources: 
            ResourceList.Add(DateTime.UtcNow.Millisecond.ToString());
            ResourceList.Add(DateTime.UtcNow.Second.ToString());
            ResourceList.Add(DateTime.UtcNow.ToLongDateString());

            // example of subscribe any
            //_messaging.SubscribeAnyRequestAsync(OnRequest);

            // example of subscribe per request
            //_messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
            //_messaging.SubscribeRequestAsync<TotalCountRequest>(OnTotalCountRequestReceived);

            // exemple of subscribe per handler (subscribe any request that has a handler defined)
            //_messaging.SubscribeHandlers();
        }

        public async Task RunListResource_UsingHandler()
        {
            await _messaging.SubscribeHandler<ListResourceHandler, ListResourceRequest>();

            var testBuffer = _serviceProvider.GetService<ListResourceRequestBuffer>();
            testBuffer.timeoutMs = 1500;
            var response = await testBuffer.SendRequestAsync();

            _logger.LogInformation($"Resource list from all instances:");
            foreach(var resource in response.ResourceList)
            {
                _logger.LogInformation(resource);
            }

            await _messaging.UnsubscribeRequestAsync<ListResourceRequest>();
        }

        /// <summary>
        /// Verify that a request can be freely subcribed/unsubcribed
        /// </summary>
        /// <returns></returns>
        public async Task Test_Sub_Unsub_Resub()
        {
            for (int i = 0; i < 100; i++)
            {
                await _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
                await _messaging.UnsubscribeRequestAsync<HelloWorldRequest>();
            }
            await _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);

            // Check sub result with 1 call
            var testBuffer = _serviceProvider.GetService<HelloWorldRequestBuffer>();
            testBuffer.timeoutMs = 1500;
            var response = await testBuffer.SendRequestAsync();
            _logger.LogInformation($"HelloWorld after multiple sub/unsub: " + response.InstanceResponse);

            // Check single sub remaining
            _messaging.RequestDelegateCollection.TryGetValue($"{typeof(HelloWorldRequest)}", out var temp);
            if (temp == OnHelloWorldRequestReceived && _messaging.RequestDelegateCollection.Count == 1)
                _logger.LogInformation($"{nameof(Test_Sub_Unsub_Resub)} Single sub: SUCCESS");
            else
                _logger.LogInformation($"{nameof(Test_Sub_Unsub_Resub)} Single sub: FAILURE");

            await _messaging.UnsubscribeRequestAsync<HelloWorldRequest>();
        }

        /// <summary>
        /// Verify that any can be freely subcribed/unsubcribed
        /// </summary>
        /// <returns></returns>
        public async Task Test_Sub_Unsub_Resub2()
        {
            for (int i = 0; i < 100; i++)
            {
                await _messaging.SubscribeAnyRequestAsync((object e, ReceivedEventArgs args) => { test_count++; });
                await _messaging.UnsubscribeAnyRequestAsync();
            }
            await _messaging.SubscribeAnyRequestAsync((object e, ReceivedEventArgs args) => { test_count++; });

            var testBuffer = _serviceProvider.GetService<HelloWorldRequestBuffer>();
            testBuffer.timeoutMs = 2000;
            var response = await testBuffer.SendRequestAsync();
            if (test_count == 1)
                _logger.LogInformation($"{nameof(Test_Sub_Unsub_Resub2)} Single sub: SUCCESS");
            else
                _logger.LogInformation($"{nameof(Test_Sub_Unsub_Resub2)} Single sub: FAILURE");

            test_count = 0;
            await _messaging.UnsubscribeAnyRequestAsync();
        }
        private static int test_count = 0;

        /// <summary>
        /// Hello world example: No input in request
        /// </summary>
        public async Task RunHelloWorld()
        {
            await _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
            _logger.LogTrace("Performing HelloWorld process");

            var buffer = _serviceProvider.GetRequiredService<HelloWorldRequestBuffer>();
            var response = await buffer.SendRequestAsync();

            _logger.LogTrace($"Response from several apps:\r\n{response.InstanceResponse}");
            _logger.LogInformation($"HelloWorld test: " + response.InstanceResponse);
            await _messaging.UnsubscribeRequestAsync<HelloWorldRequest>();
        }

        /// <summary>
        /// Hello world example: using Handler
        /// </summary>
        public async Task RunHelloWorl_UsingHandler()
        {
            await _messaging.SubscribeHandler<HelloWorldHandler, HelloWorldRequest>();

            _logger.LogTrace("Performing HelloWorld process");

            var buffer = _serviceProvider.GetRequiredService<HelloWorldRequestBuffer>();
            var response = await buffer.SendRequestAsync();

            _logger.LogTrace($"Response from several apps:\r\n{response.InstanceResponse}");
            _logger.LogInformation($"HelloWorld test: \r\n" + response.InstanceResponse);

            await _messaging.UnsubscribeRequestAsync<HelloWorldRequest>();
        }

        /// <summary>
        /// Total Count example: using Handler
        /// </summary>
        public async Task RunTotalCount_UsingHandler()
        {
            await _messaging.SubscribeHandler<TotalCountHandler, TotalCountRequest>();

            _logger.LogTrace("Performing HelloWorld process");

            var buffer = _serviceProvider.GetRequiredService<TotalCountRequestBuffer>();
            var response = await buffer.SendRequestAsync();

            _logger.LogInformation($"TotalCount test (from a handler) : (expected 100 + 5 per instance running). RESULT: " + response.Count);

            await _messaging.UnsubscribeRequestAsync<TotalCountRequest>();
        }

        /// <summary>
        /// Total Count example: input in request
        /// </summary>
        public async Task RunTotalCount()
        {
            await _messaging.SubscribeRequestAsync<TotalCountRequest>(OnTotalCountRequestReceived);
            _logger.LogTrace("Performing TotalCount process");

            var buffer = _serviceProvider.GetRequiredService<TotalCountRequestBuffer>();
            buffer.Request.InitialCount = 100;
            var response = await buffer.SendRequestAsync();

            _logger.LogTrace($"Total count among all the apps: {response.Count}");
            await _messaging.UnsubscribeRequestAsync<TotalCountRequest>();
            _logger.LogInformation($"TotalCount test: (expected 100 + 5 per instance running). RESULT: " + response.Count);
        }

        /// <summary>
        /// Attempt various forbidden subscription
        /// </summary>
        /// <returns></returns>
        public async Task DoingShitOnPurpose()
        {

            _logger.LogInformation("Subscring to Any Request.");
            await _messaging.SubscribeAnyRequestAsync(OnRequest);
            try
            {
                _logger.LogInformation($"Subscribing to Any Request a 2nd time. Expecting failure...");
                await _messaging.SubscribeAnyRequestAsync(OnRequest);
            }
            catch (SubscriptionException ex)
            {
                _logger.LogInformation($"EXCEPTION: {ex.Message}");
            }

            try
            {
                _logger.LogInformation($"Subscribing to {nameof(HelloWorldRequest)}. Expecting failure...");
                await _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
            }
            catch (SubscriptionException ex)
            {
                _logger.LogInformation($"EXCEPTION: {ex.Message}");
            }

            await _messaging.UnsubscribeAnyRequestAsync();
            _logger.LogInformation($"Subscription cleared.");


            _logger.LogInformation($"Subscribing to {nameof(HelloWorldRequest)}.");
            await _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
            try
            {
                _logger.LogInformation($"Subscribing to {nameof(HelloWorldRequest)}. Expecting failure...");
                await _messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
            }
            catch (SubscriptionException ex)
            {
                _logger.LogInformation($"EXCEPTION: {ex.Message}");
            }

            _logger.LogInformation($"Subscribing to {nameof(TotalCountRequest)}.");
            await _messaging.SubscribeRequestAsync<TotalCountRequest>(OnTotalCountRequestReceived);
            try
            {
                _logger.LogInformation($"Subscribing to {nameof(TotalCountRequest)}. Expecting failure...");
                await _messaging.SubscribeRequestAsync<TotalCountRequest>(OnTotalCountRequestReceived);
            }
            catch (SubscriptionException ex)
            {
                _logger.LogInformation($"EXCEPTION: {ex.Message}");
            }

            try
            {
                _logger.LogInformation($"Subscribing handlers. Expecting failure...");
                await _messaging.SubscribeHandlers();
            }
            catch (SubscriptionException ex)
            {
                _logger.LogInformation($"EXCEPTION: {ex.Message}");
            }

            await _messaging.UnsubscribeRequestAsync<HelloWorldRequest>();
            await _messaging.UnsubscribeRequestAsync<TotalCountRequest>();
            _logger.LogInformation($"Subscription cleared.");

            _logger.LogInformation($"Subscribing handlers");
            await _messaging.SubscribeHandlers();
            try
            {
                _logger.LogInformation($"Subscribing to Any Request. Expecting failure...");
                await _messaging.SubscribeAnyRequestAsync(OnRequest);
            }
            catch (SubscriptionException ex)
            {
                _logger.LogInformation($"EXCEPTION: {ex.Message}");
            }
            await _messaging.UnsubscribeRequestAsync<HelloWorldRequest>();
            _logger.LogInformation($"Subscription cleared.");
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
            _logger.LogTrace($"Method: {nameof(OnHelloWorldRequestReceived)}");
            await _messaging.PublishResponseAsync(correlationId, new HelloWorldResponse(correlationId)
            {
                InstanceResponse = $"Hello from {Environment.UserName}"
            });
        }

        private async void OnTotalCountRequestReceived(string correlationId, TotalCountRequest request)
        {
            _logger.LogTrace($"Method: {nameof(OnTotalCountRequestReceived)}");
            await _messaging.PublishResponseAsync(correlationId, new TotalCountResponse(correlationId, personalCount));
        }
    }
}
