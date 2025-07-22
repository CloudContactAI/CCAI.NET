using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;

namespace CCAI.NET.Examples;

public class MmsSend
{
    public static async Task RunAsync()
    {
        var config = new CCAIConfig
        {
            ClientId = "2682",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
        };
        
        using var ccai = new CCAIClient(config);
        
        var imagePath = "/Users/andreas/CCAI.NET/imageNET.jpg";
        var contentType = "image/jpeg";
        
        try
        {
            Console.WriteLine($"Checking image file: {imagePath}");
            if (!System.IO.File.Exists(imagePath))
            {
                Console.WriteLine("Image file not found!");
                return;
            }
            
            Console.WriteLine("Sending MMS with image...");
            
            var account = new Account
            {
                FirstName = "Andreas",
                LastName = "User",
                Phone = "+14156961732"
            };
            
            var options = new SMSOptions
            {
                OnProgress = status => Console.WriteLine($"Progress: {status}")
            };
            
            var response = await ccai.MMS.SendWithImageAsync(
                imagePath: imagePath,
                contentType: contentType,
                accounts: new[] { account },
                message: "Hello ${FirstName}! Check out this image from your CCAI.NET library.",
                title: "MMS Test",
                options: options
            );
            
            Console.WriteLine("MMS sent successfully!");
            Console.WriteLine($"Status: {response.Status}");
            Console.WriteLine($"Campaign ID: {response.CampaignId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner error: {ex.InnerException.Message}");
            }
        }
    }
}