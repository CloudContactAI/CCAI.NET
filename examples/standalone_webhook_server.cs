using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CCAI.NET.Examples
{
    public class StandaloneWebhookServer
    {
        public static async Task RunAsync()
        {
            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();

            // Configure the webhook endpoint
            app.MapPost("/webhook", async (HttpContext context) =>
            {
                Console.WriteLine("Received webhook event!");
                
                // Read the request body
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                
                // Log headers
                Console.WriteLine("Headers:");
                foreach (var header in context.Request.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {header.Value}");
                }
                
                // Log body
                Console.WriteLine("Body:");
                Console.WriteLine(body);
                
                // Return a success response
                return Results.Ok("Webhook received");
            });

            // Configure a GET endpoint for testing
            app.MapGet("/webhook", (HttpContext context) =>
            {
                Console.WriteLine("Received GET request to webhook endpoint");
                return Results.Ok("Webhook endpoint is working");
            });

            // Configure a root endpoint
            app.MapGet("/", () => "Webhook server is running. Send POST requests to /webhook");

            // Set the server to listen on port 3000
            app.Urls.Add("http://localhost:3000");

            Console.WriteLine("Starting webhook server on http://localhost:3000");
            Console.WriteLine("Press Ctrl+C to stop the server");

            await app.RunAsync();
        }
    }
}
