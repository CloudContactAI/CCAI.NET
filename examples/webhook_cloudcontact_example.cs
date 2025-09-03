using System;
using System.Text.Json;
using CCAI.NET;
using CCAI.NET.Webhook;

namespace CCAI.NET.Examples;

public class CloudContactWebhookExample
{
    public static void RunAsync()
    {
        // Example webhook payloads from CloudContact
        
        // 1. Message Sent Event
        var messageSentJson = """
        {
          "eventType": "message.sent",
          "data": {
            "SmsSid": 12345,
            "MessageStatus": "DELIVERED",
            "To": "+1234567890",
            "Message": "Hello! Your order #12345 has been shipped.",
            "CustomData": "order_id:12345,customer_type:premium",
            "ClientExternalId": "customer_abc123",
            "CampaignId": 67890,
            "CampaignTitle": "Order Notifications",
            "Segments": 2,
            "TotalPrice": 0.02
          }
        }
        """;
        
        // 2. Message Incoming Event
        var messageIncomingJson = """
        {
          "eventType": "message.incoming",
          "data": {
            "SmsSid": 0,
            "MessageStatus": "RECEIVED",
            "To": "+0987654321",
            "Message": "Yes, I'm interested in learning more!",
            "CustomData": "",
            "ClientExternalId": "customer_abc123",
            "CampaignId": 67890,
            "CampaignTitle": "Lead Generation Campaign",
            "From": "+1234567890"
          }
        }
        """;
        
        // 3. Message Excluded Event
        var messageExcludedJson = """
        {
          "eventType": "message.excluded",
          "data": {
            "SmsSid": 0,
            "MessageStatus": "EXCLUDED",
            "To": "+1234567890",
            "Message": "Hi {{name}}, check out our new products!",
            "CustomData": "lead_source:website,segment:new_users",
            "ClientExternalId": "customer_xyz789",
            "CampaignId": 67890,
            "CampaignTitle": "Product Launch Campaign",
            "ExcludedReason": "Duplicate phone number in campaign"
          }
        }
        """;
        
        // 4. Carrier Error Event
        var carrierErrorJson = """
        {
          "eventType": "message.error.carrier",
          "data": {
            "SmsSid": 12345,
            "MessageStatus": "FAILED",
            "To": "+1234567890",
            "Message": "Your verification code is: 123456",
            "CustomData": "verification_attempt:1",
            "ClientExternalId": "user_def456",
            "CampaignId": 0,
            "CampaignTitle": "",
            "ErrorCode": "30008",
            "ErrorMessage": "Unknown destination handset",
            "ErrorType": "carrier"
          }
        }
        """;
        
        // 5. CloudContact Error Event
        var cloudContactErrorJson = """
        {
          "eventType": "message.error.cloudcontact",
          "data": {
            "SmsSid": 12345,
            "MessageStatus": "FAILED",
            "To": "+1234567890",
            "Message": "Welcome to our service!",
            "CustomData": "signup_source:landing_page",
            "ClientExternalId": "new_user_ghi789",
            "CampaignId": 67890,
            "CampaignTitle": "Welcome Series",
            "ErrorCode": "CCAI-001",
            "ErrorMessage": "Insufficient account balance",
            "ErrorType": "cloudcontact"
          }
        }
        """;
        
        // Create webhook service (you would normally get this from CCAIClient)
        var config = new CCAIConfig
        {
            ClientId = "your-client-id",
            ApiKey = "your-api-key"
        };
        
        using var client = new CCAIClient(config);
        var webhookService = client.Webhook;
        
        // Parse and handle different event types
        ProcessWebhookEvent(webhookService, messageSentJson);
        ProcessWebhookEvent(webhookService, messageIncomingJson);
        ProcessWebhookEvent(webhookService, messageExcludedJson);
        ProcessWebhookEvent(webhookService, carrierErrorJson);
        ProcessWebhookEvent(webhookService, cloudContactErrorJson);
    }
    
    static void ProcessWebhookEvent(WebhookService webhookService, string json)
    {
        try
        {
            // Parse the CloudContact webhook event
            var cloudContactEvent = webhookService.ParseCloudContactEvent(json);
            
            Console.WriteLine($"Event Type: {cloudContactEvent.EventType}");
            Console.WriteLine($"Message Status: {cloudContactEvent.Data.MessageStatus}");
            Console.WriteLine($"To: {cloudContactEvent.Data.To}");
            Console.WriteLine($"Message: {cloudContactEvent.Data.Message}");
            
            // Handle different event types
            switch (cloudContactEvent.EventType)
            {
                case "message.sent":
                    HandleMessageSent(cloudContactEvent.Data);
                    break;
                    
                case "message.incoming":
                    HandleMessageIncoming(cloudContactEvent.Data);
                    break;
                    
                case "message.excluded":
                    HandleMessageExcluded(cloudContactEvent.Data);
                    break;
                    
                case "message.error.carrier":
                    HandleCarrierError(cloudContactEvent.Data);
                    break;
                    
                case "message.error.cloudcontact":
                    HandleCloudContactError(cloudContactEvent.Data);
                    break;
                    
                default:
                    Console.WriteLine($"Unknown event type: {cloudContactEvent.EventType}");
                    break;
            }
            
            Console.WriteLine("---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing webhook: {ex.Message}");
        }
    }
    
    static void HandleMessageSent(WebhookEventData data)
    {
        Console.WriteLine($"✅ Message delivered successfully!");
        Console.WriteLine($"   SMS ID: {data.SmsSid}");
        Console.WriteLine($"   Segments: {data.Segments}");
        Console.WriteLine($"   Cost: ${data.TotalPrice:F4}");
        
        if (data.CampaignId > 0)
        {
            Console.WriteLine($"   Campaign: {data.CampaignTitle} (ID: {data.CampaignId})");
        }
    }
    
    static void HandleMessageIncoming(WebhookEventData data)
    {
        Console.WriteLine($"📨 Received reply from {data.From}");
        Console.WriteLine($"   Reply: {data.Message}");
        
        if (data.CampaignId > 0)
        {
            Console.WriteLine($"   Original Campaign: {data.CampaignTitle}");
        }
        
        // Here you could implement auto-responses, lead scoring, etc.
    }
    
    static void HandleMessageExcluded(WebhookEventData data)
    {
        Console.WriteLine($"⚠️  Message excluded from campaign");
        Console.WriteLine($"   Reason: {data.ExcludedReason}");
        Console.WriteLine($"   Campaign: {data.CampaignTitle}");
        
        // Track exclusion reasons for campaign optimization
    }
    
    static void HandleCarrierError(WebhookEventData data)
    {
        Console.WriteLine($"❌ Carrier delivery failed");
        Console.WriteLine($"   Error Code: {data.ErrorCode}");
        Console.WriteLine($"   Error: {data.ErrorMessage}");
        
        // Implement retry logic or mark number as invalid
        if (data.ErrorCode == "30008")
        {
            Console.WriteLine("   → Number appears to be invalid, consider removing from list");
        }
    }
    
    static void HandleCloudContactError(WebhookEventData data)
    {
        Console.WriteLine($"🚨 CloudContact system error");
        Console.WriteLine($"   Error Code: {data.ErrorCode}");
        Console.WriteLine($"   Error: {data.ErrorMessage}");
        
        // Handle system-level errors
        if (data.ErrorCode == "CCAI-001")
        {
            Console.WriteLine("   → Account balance low, please add funds");
        }
        else if (data.ErrorCode == "CCAI-002")
        {
            Console.WriteLine("   → Account suspended, contact support");
        }
    }
}
