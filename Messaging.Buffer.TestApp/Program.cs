using Messaging.Buffer.Service;
using Messaging.Buffer.TestApp;
using Messaging.Buffer.TestApp.Handlers;
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
            .AddMessagingBuffer(Configuration, "Redis", (cfg) =>
            {
                cfg.AddBuffer<HelloWorldRequestBuffer, HelloWorldRequest, HelloWorldResponse, HelloWorldHandler>();
                cfg.AddBuffer<TotalCountRequestBuffer, TotalCountRequest, TotalCountResponse, TotalCountHandler>();
                cfg.AddBuffer<ListResourceRequestBuffer, ListResourceRequest, ListResourceResponse, ListResourceHandler>();
            })

            .BuildServiceProvider();

        var app = serviceProvider.GetService<Application>();

        Console.WriteLine("***********  RunListResource_UsingHandler ************");
        await app.RunListResource_UsingHandler();

        Console.WriteLine("***********  Test_Sub_Unsub_Resub ************");
        await app.Test_Sub_Unsub_Resub();

        Console.WriteLine("***********  Test_Sub_Unsub_Resub2 ************");
        await app.Test_Sub_Unsub_Resub2();

        Console.WriteLine("***********  RunHelloWorl_UsingHandler ************");
        await app.RunHelloWorl_UsingHandler();
        await app.RunHelloWorl_UsingHandler();
        await app.RunHelloWorl_UsingHandler();

        Console.WriteLine("***********  RunTotalCount_UsingHandler ************");
        await app.RunTotalCount_UsingHandler();
        await app.RunTotalCount_UsingHandler();
        await app.RunTotalCount_UsingHandler();

        Console.WriteLine("***********  RunHelloWorld ************");
        await app.RunHelloWorld();
        await app.RunHelloWorld();
        await app.RunHelloWorld();

        Console.WriteLine("***********  RunTotalCount ************");
        await app.RunTotalCount();
        await app.RunTotalCount();
        await app.RunTotalCount();

        Console.WriteLine("***********  Subscription Conflicting on purpose ************");
        await app.DoingShitOnPurpose();

        Console.WriteLine("***********  Test End ************");
        Console.WriteLine("*********************************");
        Console.WriteLine("***********  App will close in 5 min ************");
        await Task.Delay(300000); // close in 5 min
    }
}