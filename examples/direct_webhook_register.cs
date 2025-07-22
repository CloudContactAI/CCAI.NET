using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Webhook;

namespace CCAI.NET.Examples;

public class DirectWebhookRegister
{
    public static async Task RunAsync(string ngrokUrl)
    {
        // Create a CCAI client with the same configuration
        var config = new CCAIConfig
        {
            ClientId = "2682",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
        };
        
        using var ccai = new CCAIClient(config);
        
        // Define the webhook URL
        var webhookUrl = $"{ngrokUrl.TrimEnd('/')}/webhook";
        
        try
        {
            Console.WriteLine($"Registering webhook at {webhookUrl}...");
            
            // Create webhook config
            var webhookConfig = new WebhookConfig
            {
                Url = webhookUrl,
                Events = new List<WebhookEventType>
                {
                    WebhookEventType.MessageSent,
                    WebhookEventType.MessageReceived
                },
                Secret = "your-webhook-secret"
            };
            
            // Try to register using the client directly
            try
            {
                Console.WriteLine("Attempting to register webhook using CCAI client...");
                var response = await ccai.Webhook.RegisterAsync(webhookConfig);
                Console.WriteLine($"Webhook registered successfully with ID: {response.Id}");
                Console.WriteLine($"URL: {response.Url}");
                Console.WriteLine($"Events: {string.Join(", ", response.Events)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering webhook with client: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
            }
            
            // Try to list webhooks using the client
            try
            {
                Console.WriteLine("\nAttempting to list webhooks using CCAI client...");
                var webhooks = await ccai.Webhook.ListAsync();
                Console.WriteLine($"Found {webhooks.Count} webhooks");
                
                foreach (var webhook in webhooks)
                {
                    Console.WriteLine($"ID: {webhook.Id}, URL: {webhook.Url}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing webhooks with client: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                }
            }
            
            // Try direct HTTP request to the API
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Try the base URL with different paths
            var baseUrl = config.BaseUrl;
            Console.WriteLine($"\nUsing base URL: {baseUrl}");
            
            var paths = new string[]
            {
                "/webhooks",
                $"/clients/{config.ClientId}/webhooks",
                $"/client/{config.ClientId}/webhooks",
                "/webhook",
                "/hooks",
                "/notifications",
                "/subscriptions"
            };
            
            foreach (var path in paths)
            {
                var endpoint = $"{baseUrl}{path}";
                try
                {
                    Console.WriteLine($"Trying endpoint: {endpoint}");
                    
                    // Try GET request first to see if the endpoint exists
                    var getResponse = await httpClient.GetAsync(endpoint);
                    var getBody = await getResponse.Content.ReadAsStringAsync();
                    
                    Console.WriteLine($"GET Status: {getResponse.StatusCode}");
                    Console.WriteLine($"GET Response: {getBody}");
                    
                    // If GET works or returns 405 Method Not Allowed, try POST
                    if (getResponse.IsSuccessStatusCode || getResponse.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
                    {
                        var json = JsonSerializer.Serialize(webhookConfig);
                        var content = new StringContent(json, Encoding.UTF8);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        
                        var postResponse = await httpClient.PostAsync(endpoint, content);
                        var postBody = await postResponse.Content.ReadAsStringAsync();
                        
                        Console.WriteLine($"POST Status: {postResponse.StatusCode}");
                        Console.WriteLine($"POST Response: {postBody}");
                        
                        if (postResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Webhook registered successfully!");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with endpoint {endpoint}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}