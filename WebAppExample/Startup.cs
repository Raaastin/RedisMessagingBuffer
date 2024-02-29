
using Messaging.Buffer;
using Messaging.Buffer.Service;
using Microsoft.AspNetCore.Builder;
using WebAppExample.Requests;

namespace WebAppExample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("allCors", builder =>
            {
                builder.WithOrigins("*")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            // Add services to the container.
            services.AddRazorPages();
            services.AddControllers();
            services.AddMvc(o => o.EnableEndpointRouting = false);

            services.AddMessagingBuffer(Configuration, "Redis");
            services.AddSingleton<MessagingService>();

            services.AddBuffer<HelloWorldRequestBuffer, HelloWorldRequest, HelloWorldResponse>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            app.UseCors("allCors");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMvc();

            applicationLifetime.ApplicationStarted.Register(() => OnStarted(app.ApplicationServices));
        }


        private static void OnStarted(IServiceProvider serviceProvider)
        {
            var deviceHolder = serviceProvider.GetService<MessagingService>();
            deviceHolder.SubscribeHelloWorld();
        }
    }
}