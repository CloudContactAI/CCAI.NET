using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CCAI.NET.Examples;

public class CorrectWebhookRegister
{
    public static async Task RunAsync(string ngrokUrl)
    {
        // API credentials
        var apiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo";
        
        // Create HTTP client
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        // Define the webhook URL
        var webhookUrl = $"{ngrokUrl.TrimEnd('/')}/webhook";
        
        // Use the correct base URL
        var baseUrl = "https://core.cloudcontactai.com/api";
        var endpoint = $"{baseUrl}/webhooks";
        
        try
        {
            Console.WriteLine($"Registering webhook at {webhookUrl}...");
            Console.WriteLine($"Using endpoint: {endpoint}");
            
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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner error: {ex.InnerException.Message}");
            }
        }
        
        // Try to list webhooks
        try
        {
            Console.WriteLine("\nTrying to list webhooks...");
            var listEndpoint = $"{baseUrl}/webhooks";
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