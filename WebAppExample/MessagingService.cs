using Messaging.Buffer;
using Microsoft.Extensions.DependencyInjection;
using WebAppExample.Requests;

namespace WebAppExample
{
    public class MessagingService
    {
        private readonly IMessaging messaging;
        private readonly IServiceProvider serviceProvider;

        public MessagingService(IMessaging messaging, IServiceProvider serviceProvider)
        {
            this.messaging = messaging;
            this.serviceProvider = serviceProvider;
        }

        public async void SubscribeHelloWorld()
        {
            await messaging.SubscribeRequestAsync<HelloWorldRequest>(OnHelloWorldRequestReceived);
        }

        public async Task<string> RunHelloWorld()
        {
            var helloWorldBuffer = serviceProvider.GetRequiredService<HelloWorldRequestBuffer>();
            var response = await helloWorldBuffer.SendRequestAsync();
            return response.InstanceResponse;
        }

        private async void OnHelloWorldRequestReceived(string correlationId, HelloWorldRequest request)
        {
            await messaging.PublishResponseAsync(correlationId, new HelloWorldResponse(correlationId)
            {
                InstanceResponse = $"Hello from {Environment.MachineName}"
            });
        }
    }
}
