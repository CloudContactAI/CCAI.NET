// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

namespace CCAI.NET.Examples;

/// <summary>
/// Example demonstrating SMS with CustomData that should be returned in webhook callbacks
/// </summary>
public class SMSCustomDataWebhookTest
{
    public static async Task RunAsync()
    {
        try
        {
            // Load environment variables
            Env.Load();

            // Initialize the client
            var config = new CCAIConfig
            {
                ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? 
                          throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
                ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? 
                        throw new InvalidOperationException("CCAI_API_KEY not found")
            };

            using var ccai = new CCAIClient(config);

            Console.WriteLine("=== SMS Custom Data Webhook Test ===");
            Console.WriteLine($"Client ID: {config.ClientId}");
            Console.WriteLine();

            // Create SMS request with custom data
            var customAccountId = "AndreasTest123";
            var customData = "AndreasTest123";
            
            var request = SMSRequest.CreateSingle(
                firstName: "Andreas",
                lastName: "Test",
                phone: "+15551234567", // Replace with your test phone number
                message: "Hello ${FirstName}! This is a webhook test with custom data. CustomData should be: " + customData,
                title: "Custom Data Webhook Test",
                customAccountId: customAccountId,
                customData: customData
            );

            Console.WriteLine("Sending SMS with custom data...");
            Console.WriteLine($"Custom Account ID: {customAccountId}");
            Console.WriteLine($"Custom Data: {customData}");
            Console.WriteLine($"Message: {request.Message}");
            Console.WriteLine();

            // Send the SMS
            var response = await ccai.SMS.SendAsync(request);

            Console.WriteLine("SMS sent successfully!");
            Console.WriteLine($"Message ID: {response.Id}");
            Console.WriteLine($"Campaign ID: {response.CampaignId}");
            Console.WriteLine($"Status: {response.Status}");
            Console.WriteLine($"Messages Sent: {response.MessagesSent}");
            Console.WriteLine($"Timestamp: {response.Timestamp}");
            Console.WriteLine();
            
            Console.WriteLine("=== Webhook Instructions ===");
            Console.WriteLine("1. Make sure your webhook is configured in CCAI settings");
            Console.WriteLine("2. The webhook should receive the CustomData field in the callback");
            Console.WriteLine("3. Look for 'CustomData' in the webhook payload");
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
    }
}