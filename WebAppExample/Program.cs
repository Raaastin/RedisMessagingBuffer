using Messaging.Buffer.Service;
using WebAppExample.Requests;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddMvc(o => o.EnableEndpointRouting = false);

var services = builder.Services;
var configuration = builder.Configuration;
services.AddMessagingBuffer(configuration, "Redis");

services.AddBuffer<HelloWorldRequestBuffer, HelloWorldRequest, HelloWorldResponse>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseMvc();

app.Run();
