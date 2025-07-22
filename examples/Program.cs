using CCAI.NET.Examples;

// Check if we're registering a webhook or sending an SMS
if (args.Length > 0 && args[0].StartsWith("http"))
{
    // Register webhook with the provided ngrok URL
    await SimpleWebhookRegister.RunAsync(args[0]);
}
else
{
    // Send an SMS to test the webhook
    Console.WriteLine("Sending SMS to test webhook...");
    await SmsSend.RunAsync();
}
