using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Webhook;
using DotNetEnv;

namespace CCAI.NET.Examples;

public class WebhookTest
{
    public static async Task RunAsync()
    {
        Env.Load();
        
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
        };
        
        using var ccai = new CCAIClient(config);
        
        // Register webhook
        var webhookConfig = new WebhookConfig
        {
            Url = "https://your-ngrok-url.ngrok.io/webhook", // Replace with your ngrok URL
            Events = new List<WebhookEventType> { WebhookEventType.MessageSent },
            Secret = "test-secret"
        };
        
        var registration = await ccai.Webhook.RegisterAsync(webhookConfig);
        Console.WriteLine($"Webhook registered: {registration.Id}");
        
        // List webhooks to verify
        var webhooks = await ccai.Webhook.ListAsync();
        foreach (var webhook in webhooks)
        {
            Console.WriteLine($"Active webhook: {webhook.Url}");
        }
    }
}