using Messaging.Buffer.Attributes;
using Messaging.Buffer.Buffer;
using Messaging.Buffer.TestApp.Requests;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace Messaging.Buffer.TestApp.Handlers
{
    public class ListResourceHandler : HandlerBase<ListResourceRequest>
    {
        private Application _application;

        public ListResourceHandler(ILogger<ListResourceHandler> logger, IMessaging messaging, Application application) : base(logger, messaging)
        {
            _application = application;
        }

        public override async void Handle(string correlationId, ListResourceRequest request)
        {
            _logger.LogTrace($"Handle: {nameof(ListResourceRequest)}");


            await base.Respond(correlationId, new ListResourceResponse(correlationId)
            {
                ResourceList = _application.ResourceList
            });
        }
    }
}
