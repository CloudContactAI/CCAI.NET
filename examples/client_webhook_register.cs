using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CCAI.NET.Examples;

public class ClientWebhookRegister
{
    public static async Task RunAsync(string ngrokUrl)
    {
        // API credentials
        var clientId = "YOUR_CLIENT_ID";
        var apiKey = "YOUR_API_KEY";
        
        // Create HTTP client
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        // Define the webhook URL
        var webhookUrl = $"{ngrokUrl.TrimEnd('/')}/webhook";
        
        // Try different API endpoints and formats
        var endpoints = new string[]
        {
            $"https://core.cloudcontactai.com/api/clients/{clientId}/webhooks",
            $"https://core.cloudcontactai.com/api/v1/clients/{clientId}/webhooks",
            $"https://core.cloudcontactai.com/api/client/{clientId}/webhooks",
            $"https://core.cloudcontactai.com/api/v1/client/{clientId}/webhooks"
        };
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                Console.WriteLine($"Trying endpoint: {endpoint}");
                
                // Try different payload formats
                var payloads = new object[]
                {
                    // Format 1: Standard format
                    new
                    {
                        url = webhookUrl,
                        events = new[] { "message.sent", "message.received" },
                        secret = "your-webhook-secret"
                    },
                    
                    // Format 2: With client ID
                    new
                    {
                        clientId = clientId,
                        url = webhookUrl,
                        events = new[] { "message.sent", "message.received" },
                        secret = "your-webhook-secret"
                    },
                    
                    // Format 3: With webhook object
                    new
                    {
                        webhook = new
                        {
                            url = webhookUrl,
                            events = new[] { "message.sent", "message.received" },
                            secret = "your-webhook-secret"
                        }
                    }
                };
                
                foreach (var payload in payloads)
                {
                    try
                    {
                        Console.WriteLine($"Trying payload format: {JsonSerializer.Serialize(payload)}");
                        
                        var json = JsonSerializer.Serialize(payload);
                        var content = new StringContent(json, Encoding.UTF8);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        
                        // Send request
                        var response = await httpClient.PostAsync(endpoint, content);
                        
                        // Check response
                        var responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Status: {response.StatusCode}");
                        Console.WriteLine($"Response: {responseBody}");
                        
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Webhook registered successfully!");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error with payload: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with endpoint {endpoint}: {ex.Message}");
            }
        }
        
        // Try to list webhooks from different endpoints
        Console.WriteLine("\nTrying to list webhooks from different endpoints...");
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                var listResponse = await httpClient.GetAsync(endpoint);
                var listBody = await listResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Endpoint: {endpoint}");
                Console.WriteLine($"Status: {listResponse.StatusCode}");
                Console.WriteLine($"Response: {listBody}");
                
                if (listResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Found webhooks endpoint!");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing webhooks from {endpoint}: {ex.Message}");
            }
        }
    }
}