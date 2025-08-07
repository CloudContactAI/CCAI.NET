using System;
using System.Threading.Tasks;
using CCAI.NET;
using DotNetEnv;

class Program
{
    static async Task Main()
    {
        DotNetEnv.Env.Load();
        
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
        };
        
        using var ccai = new CCAIClient(config);
        
        // List existing webhooks
        var webhooks = await ccai.Webhook.ListAsync();
        Console.WriteLine($"Found {webhooks.Count} webhooks:");
        foreach (var webhook in webhooks)
        {
            Console.WriteLine($"- ID: {webhook.Id}, URL: {webhook.Url}");
        }
        
        // Send SMS to test webhook
        Console.WriteLine("\nSending SMS...");
        var response = await ccai.SMS.SendSingleAsync(
            firstName: "Andreas",
            lastName: "User", 
            phone: "+14156961732",
            message: "Hello ${FirstName}! Testing webhook.",
            title: "Webhook Test"
        );
        
        Console.WriteLine($"SMS sent: {response.Status}");
    }
}