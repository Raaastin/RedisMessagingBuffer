using Messaging.Buffer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebAppExample.Requests;

namespace WebAppExample.Controllers
{
    [Route("app")]
    public class HelloWorldController : Controller
    {
        private readonly MessagingService messagingService;

        public HelloWorldController(MessagingService messagingService)
        {
            this.messagingService = messagingService;
        }

        [HttpGet("hello-world")]
        public async Task<string> RunHelloWorld()
        {
            return await messagingService.RunHelloWorld();
        }
    }
}
