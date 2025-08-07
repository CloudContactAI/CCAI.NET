using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Webhook;
using DotNetEnv;

namespace CCAI.NET.Examples;

public class RegisterWebhook
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
        
        var webhookConfig = new WebhookConfig
        {
            Url = "https://49a06ede2fd6.ngrok-free.app/webhook",
            Events = new List<WebhookEventType> { WebhookEventType.MessageSent },
            Secret = "test-secret"
        };
        
        var registration = await ccai.Webhook.RegisterAsync(webhookConfig);
        Console.WriteLine($"Webhook registered: {registration.Id}");
    }
}