using CCAI.NET;
using CCAI.NET.SMS;

namespace CCAI.NET.Examples;

/// <summary>
/// Example with progress tracking using the CCAI.NET client
/// </summary>
public class ProgressTrackingExample
{
    /// <summary>
    /// Run the progress tracking example
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a new CCAI client
        var config = new CCAIConfig
        {
            ClientId = "YOUR-CLIENT-ID",
            ApiKey = "API-KEY-TOKEN"
        };
        
        using var ccai = new CCAIClient(config);
        
        // Example recipient
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            Phone = "+15551234567"  // Use E.164 format
        };
        
        // Message with variable placeholders
        var message = "Hello ${FirstName} ${LastName}, this is a test message with progress tracking!";
        var title = "Progress Tracking Test";
        
        // Create options with progress tracking
        var options = new SMSOptions
        {
            Timeout = 60,
            Retries = 3,
            OnProgress = status => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {status}")
        };
        
        try
        {
            Console.WriteLine("Starting SMS send with progress tracking...");
            
            // Send SMS with progress tracking
            var response = await ccai.SMS.SendAsync(
                accounts: new[] { account },
                message: message,
                title: title,
                options: options
            );
            
            Console.WriteLine("\nSMS sent successfully!");
            Console.WriteLine($"Response ID: {response.Id}");
            Console.WriteLine($"Status: {response.Status}");
            
            if (response.CampaignId != null)
            {
                Console.WriteLine($"Campaign ID: {response.CampaignId}");
            }
            
            if (response.MessagesSent.HasValue)
            {
                Console.WriteLine($"Messages sent: {response.MessagesSent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
