using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;

namespace CCAI.NET.Examples;

public class SmsSend
{
    public static async Task RunAsync()
    {
        var config = new CCAIConfig
        {
            ClientId = "YOUR_CLIENT_ID",
            ApiKey = "YOUR_API_KEY"
        };
        
        using var ccai = new CCAIClient(config);
        
        try
        {
            Console.WriteLine("Sending SMS...");
            var response = await ccai.SMS.SendSingleAsync(
                firstName: "Andreas",
                lastName: "User",
                phone: "+1234567890",  // Replace with your phone number
                message: "Hello ${FirstName}! This is a test message from your CCAI.NET library.",
                title: "Test Message"
            );
            
            Console.WriteLine("SMS sent successfully!");
            Console.WriteLine($"Status: {response.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}