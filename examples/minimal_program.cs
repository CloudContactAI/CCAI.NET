using System;
using System.Threading.Tasks;
using CCAI.NET.Examples;

// Simple program to register webhook or send SMS
if (args.Length > 0 && args[0].StartsWith("http"))
{
    // Register webhook with the provided ngrok URL using direct approach
    await DirectWebhookRegister.RunAsync(args[0]);
}
else
{
    // Send an SMS to test the webhook
    Console.WriteLine("Sending SMS to test webhook...");
    await SmsSend.RunAsync();
}