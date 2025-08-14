using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Webhook;
using DotNetEnv;

namespace CCAI.NET.Examples;

public class TestWebhook
{
    public static async Task RunAsync(string ngrokUrl)
    {
        if (string.IsNullOrEmpty(ngrokUrl))
        {
            Console.WriteLine("Usage: Please provide your ngrok URL as an argument");
            Console.WriteLine("Example: dotnet run https://abc123.ngrok.io");
            return;
        }

        Env.Load();
        
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found"),
            UseTestEnvironment = true
        };
        
        using var ccai = new CCAIClient(config);
        
        try
        {
            Console.WriteLine($"Registering webhook with URL: {ngrokUrl}");
            
            var webhookConfig = new WebhookConfig
            {
                Url = ngrokUrl,
                Events = new[] { WebhookEventType.MessageSent, WebhookEventType.MessageReceived }
            };
            
            var registration = await ccai.Webhook.RegisterAsync(webhookConfig);
            
            Console.WriteLine($"Webhook registered successfully!");
            Console.WriteLine($"Webhook ID: {registration.Id}");
            Console.WriteLine($"URL: {registration.Url}");
            Console.WriteLine($"Events: {string.Join(", ", registration.Events)}");
            
            Console.WriteLine("\nNow send an SMS to test the webhook!");
            Console.WriteLine("Check your webhook server terminal for incoming webhook events.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
