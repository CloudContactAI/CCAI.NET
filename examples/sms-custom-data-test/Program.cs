// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;
using System.Text.Json;

// Load environment variables from current directory
Env.Load("./.env");

// Initialize the client
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? 
              throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? 
            throw new InvalidOperationException("CCAI_API_KEY not found"),
    UseTestEnvironment = true
};

using var ccai = new CCAIClient(config);

Console.WriteLine("=== SMS Custom Data Webhook Test ===");
Console.WriteLine($"Client ID: {config.ClientId}");
Console.WriteLine($"API Key (first 20 chars): {config.ApiKey[..20]}...");
Console.WriteLine($"Base URL: {ccai.GetBaseUrl()}");
Console.WriteLine($"Email Base URL: {ccai.GetEmailBaseUrl()}");
Console.WriteLine($"Auth Base URL: {ccai.GetAuthBaseUrl()}");
Console.WriteLine($"Test Environment: {config.UseTestEnvironment}");
Console.WriteLine();

try
{
    // Create SMS with custom data using the working SendSingleAsync method
    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    var uniqueId = Guid.NewGuid().ToString()[..8];
    var customAccountId = $"TestAccount_{uniqueId}";
    var customData = $"WebhookTestData_{uniqueId}";
    var message = $"Hello ${{FirstName}}! This is a webhook test with custom data at {timestamp}.";
    
    Console.WriteLine("Sending SMS with custom data...");
    Console.WriteLine($"Custom Account ID: {customAccountId}");
    Console.WriteLine($"Custom Data: {customData}");
    Console.WriteLine($"Message: {message}");
    Console.WriteLine();

    // Send the SMS using the working method
    var response = await ccai.SMS.SendSingleAsync(
        firstName: "John",
        lastName: "Doe",
        phone: "+14155551212",      // Replace with your test phone number
        message: message,
        title: $"Webhook Test Campaign {uniqueId}",
        customData: customData
    );

    Console.WriteLine("SMS sent successfully!");
    Console.WriteLine($"Message ID: {response.Id}");
    Console.WriteLine($"Campaign ID: {response.CampaignId}");
    Console.WriteLine($"Status: {response.Status}");
    Console.WriteLine($"Messages Sent: {response.MessagesSent}");
    Console.WriteLine($"Timestamp: {response.Timestamp}");
    Console.WriteLine();
    
    // Dump the full response JSON
    Console.WriteLine("=== Full Response JSON ===");
    var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(responseJson);
    Console.WriteLine();
    
    Console.WriteLine("=== Webhook Instructions ===");
    Console.WriteLine("1. Make sure your webhook is configured in CCAI settings");
    Console.WriteLine("2. The webhook should receive the CustomData field in the callback");
    Console.WriteLine("3. Look for the custom data values in the webhook payload");
    Console.WriteLine("4. The webhook payload should include both delivery and response notifications");
    Console.WriteLine();
    Console.WriteLine("Expected webhook payload should contain:");
    Console.WriteLine($"- CustomData: {customData}");
    Console.WriteLine($"- CustomAccountId: {customAccountId}");
    Console.WriteLine("- Message delivery status");
    Console.WriteLine("- Any reply messages from the recipient");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
    }
}