using System;
using System.Threading.Tasks;
using CCAI.NET;
using DotNetEnv;

namespace CCAI.NET.Examples;

public class CheckWebhooks
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
        
        var webhooks = await ccai.Webhook.ListAsync();
        Console.WriteLine($"Found {webhooks.Count} webhooks:");
        
        foreach (var webhook in webhooks)
        {
            Console.WriteLine($"- ID: {webhook.Id}, URL: {webhook.Url}");
        }
    }
}