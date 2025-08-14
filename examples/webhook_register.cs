using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Webhook;

namespace CCAI.NET.Examples;

public class WebhookRegister
{
    public static async Task RunAsync(string ngrokUrl)
    {
        if (string.IsNullOrEmpty(ngrokUrl))
        {
            Console.WriteLine("Please provide your ngrok URL as a command line argument");
            Console.WriteLine("Example: dotnet run --project examples.csproj -- https://your-ngrok-url.ngrok.io");
            return;
        }
        
        var config = new CCAIConfig
        {
            ClientId = "2682",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
        };
        
        using var ccai = new CCAIClient(config);
        
        try
        {
            // Define the webhook URL (your ngrok URL + /webhook)
            var webhookUrl = $"{ngrokUrl.TrimEnd('/')}/webhook";
            
            // Define the webhook configuration
            var webhookConfig = new WebhookConfig
            {
                Url = webhookUrl,
                Events = new List<WebhookEventType>
                {
                    WebhookEventType.MessageSent,
                    WebhookEventType.MessageReceived
                },
                Secret = "your-webhook-secret" // Use the same secret as in webhook_handler.cs
            };
            
            Console.WriteLine($"Registering webhook at {webhookUrl}...");
            
            // Register the webhook
            var registration = await ccai.Webhook.RegisterAsync(webhookConfig);
            
            Console.WriteLine($"Webhook registered successfully with ID: {registration.Id}");
            Console.WriteLine("Webhook details:");
            Console.WriteLine($"  URL: {registration.Url}");
            Console.WriteLine($"  Events: {string.Join(", ", registration.Events)}");
            
            // List all webhooks
            Console.WriteLine("\nAll registered webhooks:");
            var webhooks = await ccai.Webhook.ListAsync();
            
            foreach (var webhook in webhooks)
            {
                Console.WriteLine($"  ID: {webhook.Id}, URL: {webhook.Url}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering webhook: {ex.Message}");
        }
    }
    
    public static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            await RunAsync(args[0]);
        }
        else
        {
            await RunAsync(string.Empty);
        }
    }
}