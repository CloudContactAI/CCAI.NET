using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Email;

namespace CCAI.NET.Examples;

public class TestEmailSender
{
    public static async Task RunAsync()
    {
        // Use test environment configuration
        var config = new CCAIConfig
        {
            ClientId = "1231",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJhbmRyZWFzQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzUyMDg5MDk2LCJpYXQiOjE3NTIwODkwOTYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjEyMzEsImlkIjoxMjIzLCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiIzNTAxZjVmNC0zOWYyLTRjYzctYTk2Yi04ZDkyZjVlMjM5ZGUifQ.XjtDPpyYUJNJjLrpM1pdQ4Sqk90eaagqzPX2v1gwHDP1wOV4fTbB44UGDRXtWyGvN-Fz7o84_Ab-VlAjNCyEmXcDzmzscnwFSbqiZrWLAM_W3Mutd36vArl9QSG_osuYdf9T2wmAduUZu2bcnvKHdBbEaBUalJSSUoHwHsMBX3w",
            UseTestEnvironment = true
        };
        
        using var ccai = new CCAIClient(config);
        
        try
        {
            Console.WriteLine("Sending test email...");
            
            var response = await ccai.Email.SendSingleAsync(
                firstName: "Andreas",
                lastName: "AllCode",
                email: "andreas@allcode.com",
                subject: "Test Email from CCAI.NET",
                message: "<h1>Hello ${FirstName}!</h1><p>This is a test email from the CCAI.NET library using the test environment.</p>",
                senderEmail: "test@allcode.com",
                replyEmail: "test@allcode.com",
                senderName: "CCAI.NET Test",
                title: "Test Email Campaign"
            );
            
            Console.WriteLine("Email sent successfully!");
            Console.WriteLine($"Campaign ID: {response.Id}");
            Console.WriteLine($"Status: {response.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner error: {ex.InnerException.Message}");
            }
        }
    }
}