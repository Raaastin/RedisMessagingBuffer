using Messaging.Buffer.Service;
using Messaging.Buffer.TestApp;
using Messaging.Buffer.TestApp.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .Build();

        // Services
        var serviceProvider = new ServiceCollection()
            .AddSingleton<Application>()
            .AddLogging(x => { x.AddConsole(); x.SetMinimumLevel(LogLevel.Information); })

            // Register the service and any buffer
            .AddMessagingBuffer(Configuration, "Redis")
            .AddBuffer<HelloWorldRequestBuffer, HelloWorldRequest, HelloWorldResponse>()
            .AddBuffer<TotalCountRequestBuffer, TotalCountRequest, TotalCountResponse>()

            .BuildServiceProvider();

        var app = serviceProvider.GetService<Application>();

        await app.Test_Sub_Unsub_Resub();
        await app.Test_Sub_Unsub_Resub2();
        await app.RunHelloWorld();
        await app.RunTotalCount();

        await Task.Delay(300000); // close in 5 min
    }
}