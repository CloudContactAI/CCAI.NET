using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CCAI.NET;
using CCAI.NET.Webhook;

namespace CCAI.NET.Examples;

public class WebhookHandler
{
    public static async Task Main(string[] args)
    {
        // Create a builder for the web application
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container
        builder.Services.AddSingleton<CCAIClient>(sp => 
        {
            var config = new CCAIConfig
            {
                ClientId = "2682",
                ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
            };
            return new CCAIClient(config);
        });
        
        // Build the application
        var app = builder.Build();
        
        // Define a webhook secret for verification
        const string webhookSecret = "your-webhook-secret";
        
        // Configure the HTTP request pipeline
        app.UseHttpsRedirection();
        
        // Define the webhook endpoint
        app.MapPost("/webhook", async (HttpContext context) =>
        {
            // Get the CCAI client
            var ccaiClient = context.RequestServices.GetRequiredService<CCAIClient>();
            
            // Read the request body
            using var reader = new StreamReader(context.Request.Body);
            var json = await reader.ReadToEndAsync();
            
            // Get the signature from the header
            var signature = context.Request.Headers["X-CCAI-Signature"].ToString();
            
            try
            {
                // Verify the signature
                if (ccaiClient.Webhook.VerifySignature(signature, json, webhookSecret))
                {
                    // Parse the event
                    var webhookEvent = ccaiClient.Webhook.ParseEvent(json);
                    
                    // Handle different event types
                    if (webhookEvent is MessageSentEvent sentEvent)
                    {
                        Console.WriteLine($"Message sent to: {sentEvent.To}");
                        Console.WriteLine($"Message content: {sentEvent.Message}");
                        Console.WriteLine($"From: {sentEvent.From}");
                    }
                    else if (webhookEvent is MessageReceivedEvent receivedEvent)
                    {
                        Console.WriteLine($"Message received from: {receivedEvent.From}");
                        Console.WriteLine($"Message: {receivedEvent.Message}");
                    }
                    
                    return Results.Ok("Webhook received");
                }
                else
                {
                    Console.WriteLine("Invalid signature");
                    return Results.BadRequest("Invalid signature");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing webhook: {ex.Message}");
                return Results.BadRequest("Error processing webhook");
            }
        });
        
        // Start the server on port 3000
        app.Urls.Add("http://localhost:3000");
        Console.WriteLine("Starting webhook server on http://localhost:3000/webhook");
        await app.RunAsync();
    }
}