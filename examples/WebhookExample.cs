// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CCAI.NET;
using CCAI.NET.Webhook;

namespace Examples;

/// <summary>
/// Example of using the Webhook functionality
/// </summary>
public class WebhookExample
{
    /// <summary>
    /// Run the example
    /// </summary>
    public static async Task Run()
    {
        Console.WriteLine("Webhook Example");
        Console.WriteLine("--------------");
        
        // Create a CCAI client
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "YOUR_CLIENT_ID", // Replace with your client ID
            ApiKey = "YOUR_API_KEY" // Replace with your API key
        });
        
        try
        {
            // Example 1: Register a webhook
            var webhookId = await RegisterWebhook(client);
            
            if (!string.IsNullOrEmpty(webhookId))
            {
                // Example 2: List webhooks
                await ListWebhooks(client);
                
                // Example 3: Update a webhook
                await UpdateWebhook(client, webhookId);
                
                // Example 4: Delete a webhook
                await DeleteWebhook(client, webhookId);
            }
            
            // Example 5: Parse a webhook event
            ParseWebhookEvent(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 1: Register a webhook
    /// </summary>
    private static async Task<string> RegisterWebhook(CCAIClient client)
    {
        Console.WriteLine("\nRegistering a webhook...");
        
        try
        {
            var config = new WebhookConfig
            {
                Url = "https://your-webhook-endpoint.com/webhook", // Replace with your webhook endpoint
                Events = new List<WebhookEventType>
                {
                    WebhookEventType.MessageSent,
                    WebhookEventType.MessageReceived
                },
                Secret = "your-webhook-secret" // Replace with your webhook secret
            };
            
            var response = await client.Webhook.RegisterAsync(config);
            
            Console.WriteLine($"Webhook registered successfully: ID={response.Id}, URL={response.Url}");
            Console.WriteLine("Subscribed events:");
            
            foreach (var eventType in response.Events)
            {
                Console.WriteLine($"- {eventType}");
            }
            
            return response.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to register webhook: {ex.Message}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Example 2: List webhooks
    /// </summary>
    private static async Task ListWebhooks(CCAIClient client)
    {
        Console.WriteLine("\nListing webhooks...");
        
        try
        {
            var webhooks = await client.Webhook.ListAsync();
            
            Console.WriteLine($"Found {webhooks.Count} webhooks:");
            
            foreach (var webhook in webhooks)
            {
                Console.WriteLine($"- ID={webhook.Id}, URL={webhook.Url}");
                Console.WriteLine("  Subscribed events:");
                
                foreach (var eventType in webhook.Events)
                {
                    Console.WriteLine($"  - {eventType}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to list webhooks: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 3: Update a webhook
    /// </summary>
    private static async Task UpdateWebhook(CCAIClient client, string webhookId)
    {
        Console.WriteLine($"\nUpdating webhook {webhookId}...");
        
        try
        {
            var config = new WebhookConfig
            {
                Url = "https://your-updated-endpoint.com/webhook", // Replace with your updated webhook endpoint
                Events = new List<WebhookEventType>
                {
                    WebhookEventType.MessageSent // Only subscribe to message.sent events
                },
                Secret = "your-updated-secret" // Replace with your updated webhook secret
            };
            
            var response = await client.Webhook.UpdateAsync(webhookId, config);
            
            Console.WriteLine($"Webhook updated successfully: ID={response.Id}, URL={response.Url}");
            Console.WriteLine("Subscribed events:");
            
            foreach (var eventType in response.Events)
            {
                Console.WriteLine($"- {eventType}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update webhook: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 4: Delete a webhook
    /// </summary>
    private static async Task DeleteWebhook(CCAIClient client, string webhookId)
    {
        Console.WriteLine($"\nDeleting webhook {webhookId}...");
        
        try
        {
            var response = await client.Webhook.DeleteAsync(webhookId);
            
            Console.WriteLine($"Webhook deleted successfully: {response.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete webhook: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 5: Parse a webhook event
    /// </summary>
    private static void ParseWebhookEvent(CCAIClient client)
    {
        Console.WriteLine("\nParsing a webhook event...");
        
        try
        {
            // Example webhook payload for a message.sent event
            var json = @"{
                ""type"": ""message.sent"",
                ""campaign"": {
                    ""id"": 12345,
                    ""title"": ""Test Campaign"",
                    ""message"": ""Hello ${FirstName}, this is a test message."",
                    ""senderPhone"": ""+15551234567"",
                    ""createdAt"": ""2025-07-22T12:00:00Z"",
                    ""runAt"": ""2025-07-22T12:01:00Z""
                },
                ""from"": ""+15551234567"",
                ""to"": ""+15559876543"",
                ""message"": ""Hello John, this is a test message.""
            }";
            
            var webhookEvent = client.Webhook.ParseEvent(json);
            
            Console.WriteLine($"Event type: {webhookEvent.Type}");
            Console.WriteLine($"Campaign ID: {webhookEvent.Campaign.Id}");
            Console.WriteLine($"Campaign title: {webhookEvent.Campaign.Title}");
            Console.WriteLine($"From: {webhookEvent.From}");
            Console.WriteLine($"To: {webhookEvent.To}");
            Console.WriteLine($"Message: {webhookEvent.Message}");
            
            // Example of verifying a webhook signature
            var signature = "abcdef1234567890"; // This would come from the X-CCAI-Signature header
            var secret = "your-webhook-secret";
            
            var isValid = client.Webhook.VerifySignature(signature, json, secret);
            
            Console.WriteLine($"Signature valid: {isValid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse webhook event: {ex.Message}");
        }
    }
}
