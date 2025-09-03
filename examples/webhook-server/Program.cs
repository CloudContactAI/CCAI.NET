using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CCAI.NET.Webhook;

namespace CCAI.NET.Examples.WebhookServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure CORS to allow requests from ngrok
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Use CORS
            app.UseCors("AllowAll");

            // Configure the webhook endpoint for both root path and /webhook path
            app.MapPost("/", async (HttpContext context) =>
            {
                await ProcessWebhook(context, "root path");
            });

            // Also handle /webhook path for compatibility
            app.MapPost("/webhook", async (HttpContext context) =>
            {
                await ProcessWebhook(context, "/webhook path");
            });

            // Configure GET endpoints for testing
            app.MapGet("/", () => "CloudContact Webhook Server is running. Send POST requests to / or /webhook");
            app.MapGet("/webhook", () => "Webhook endpoint is working");

            // Set the server to listen on port 3000
            app.Urls.Add("http://localhost:3000");

            Console.WriteLine("Starting CloudContact webhook server on http://localhost:3000");
            Console.WriteLine("Press Ctrl+C to stop the server");

            await app.RunAsync();
        }

        private static async Task<IResult> ProcessWebhook(HttpContext context, string path)
        {
            Console.WriteLine($"🔔 Received webhook event at {path}!");
            Console.WriteLine($"⏰ Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            
            // Read the request body
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            
            // Log headers
            Console.WriteLine("📋 Headers:");
            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }
            
            // Log raw body
            Console.WriteLine("📄 Raw Body:");
            Console.WriteLine(body);
            
            // Try to parse as CloudContact webhook
            try
            {
                var webhookService = new WebhookService(null!);
                var cloudContactEvent = webhookService.ParseCloudContactEvent(body);
                
                Console.WriteLine("\n🎯 Parsed CloudContact Event:");
                Console.WriteLine($"   Event Type: {cloudContactEvent.EventType}");
                Console.WriteLine($"   Message Status: {cloudContactEvent.Data.MessageStatus}");
                Console.WriteLine($"   To: {cloudContactEvent.Data.To}");
                Console.WriteLine($"   Message: {cloudContactEvent.Data.Message}");
                
                // Handle different event types
                switch (cloudContactEvent.EventType)
                {
                    case "message.sent":
                        Console.WriteLine($"   ✅ Message delivered successfully!");
                        Console.WriteLine($"   💰 Cost: ${cloudContactEvent.Data.TotalPrice:F4}");
                        Console.WriteLine($"   📊 Segments: {cloudContactEvent.Data.Segments}");
                        if (cloudContactEvent.Data.CampaignId > 0)
                        {
                            Console.WriteLine($"   📢 Campaign: {cloudContactEvent.Data.CampaignTitle} (ID: {cloudContactEvent.Data.CampaignId})");
                        }
                        break;
                        
                    case "message.incoming":
                        Console.WriteLine($"   📨 Reply received from {cloudContactEvent.Data.From}");
                        if (cloudContactEvent.Data.CampaignId > 0)
                        {
                            Console.WriteLine($"   📢 Original Campaign: {cloudContactEvent.Data.CampaignTitle}");
                        }
                        break;
                        
                    case "message.excluded":
                        Console.WriteLine($"   ⚠️  Message excluded from campaign");
                        Console.WriteLine($"   🚫 Reason: {cloudContactEvent.Data.ExcludedReason}");
                        Console.WriteLine($"   📢 Campaign: {cloudContactEvent.Data.CampaignTitle}");
                        break;
                        
                    case "message.error.carrier":
                        Console.WriteLine($"   ❌ Carrier delivery failed");
                        Console.WriteLine($"   🔢 Error Code: {cloudContactEvent.Data.ErrorCode}");
                        Console.WriteLine($"   💬 Error: {cloudContactEvent.Data.ErrorMessage}");
                        break;
                        
                    case "message.error.cloudcontact":
                        Console.WriteLine($"   🚨 CloudContact system error");
                        Console.WriteLine($"   🔢 Error Code: {cloudContactEvent.Data.ErrorCode}");
                        Console.WriteLine($"   💬 Error: {cloudContactEvent.Data.ErrorMessage}");
                        break;
                        
                    default:
                        Console.WriteLine($"   ❓ Unknown event type: {cloudContactEvent.EventType}");
                        break;
                }
                
                if (!string.IsNullOrEmpty(cloudContactEvent.Data.CustomData))
                {
                    Console.WriteLine($"   📝 Custom Data: {cloudContactEvent.Data.CustomData}");
                }
                
                if (!string.IsNullOrEmpty(cloudContactEvent.Data.ClientExternalId))
                {
                    Console.WriteLine($"   🆔 External ID: {cloudContactEvent.Data.ClientExternalId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not parse as CloudContact webhook: {ex.Message}");
                Console.WriteLine("   This might be a legacy webhook format or invalid JSON");
            }
            
            Console.WriteLine("─────────────────────────────────────────────────────");
            
            // Return a success response
            return Results.Ok(new { status = "success", message = "Webhook processed successfully" });
        }
    }
}
