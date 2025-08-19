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
            ClientId = "2YOUR_CLIENT_ID",
            ApiKey = "YOUR_API_KEY"
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