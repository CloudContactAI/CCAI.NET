using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CCAI.NET.Examples;

public class WebhookDebug
{
    public static async Task RunAsync(string ngrokUrl)
    {
        // API credentials
        var clientId = "YOUR_CLIENT_ID";
        var apiKey = "YOUR_API_KEY";
        
        // Create HTTP client
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        // Define the webhook URL
        var webhookUrl = $"{ngrokUrl.TrimEnd('/')}/webhook";
        
        // Try different API endpoints
        var endpoints = new[]
        {
            "https://api.cloudcontactai.com/v1/webhooks",
            "https://api.cloudcontactai.com/webhooks",
            $"https://api.cloudcontactai.com/v1/clients/{clientId}/webhooks",
            $"https://api.cloudcontactai.com/clients/{clientId}/webhooks"
        };
        
        foreach (var endpoint in endpoints)
        {
            try
            {
                Console.WriteLine($"Trying endpoint: {endpoint}");
                
                // Create webhook payload
                var payload = new
                {
                    url = webhookUrl,
                    events = new[] { "message.sent", "message.received" },
                    secret = "your-webhook-secret"
                };
                
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Send request
                var response = await httpClient.PostAsync(endpoint, content);
                
                // Check response
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Response: {responseBody}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Webhook registered successfully!");
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with endpoint {endpoint}: {ex.Message}");
            }
        }
        
        // Try to list webhooks
        try
        {
            Console.WriteLine("\nTrying to list webhooks...");
            var listEndpoint = $"https://api.cloudcontactai.com/v1/clients/{clientId}/webhooks";
            var listResponse = await httpClient.GetAsync(listEndpoint);
            var listBody = await listResponse.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status: {listResponse.StatusCode}");
            Console.WriteLine($"Response: {listBody}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing webhooks: {ex.Message}");
        }
    }
}