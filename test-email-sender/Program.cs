using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Email;
using DotNetEnv;

namespace TestEmailSender;

public class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();
        
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found"),
            UseTestEnvironment = true
        };
        
        using var ccai = new CCAIClient(config);
        
        try
        {
            Console.WriteLine("Sending test email...");
            
            var response = await ccai.Email.SendSingleAsync(
                firstName: Environment.GetEnvironmentVariable("TEST_FIRST_NAME") ?? "John",
                lastName: Environment.GetEnvironmentVariable("TEST_LAST_NAME") ?? "Doe",
                email: Environment.GetEnvironmentVariable("TEST_EMAIL") ?? throw new InvalidOperationException("TEST_EMAIL not found"),
                subject: "Test Email from CCAI.NET",
                message: "<h1>Hello ${FirstName}!</h1><p>This is a test email from the CCAI.NET library using the test environment.</p>",
                senderEmail: Environment.GetEnvironmentVariable("SENDER_EMAIL") ?? throw new InvalidOperationException("SENDER_EMAIL not found"),
                replyEmail: Environment.GetEnvironmentVariable("REPLY_EMAIL") ?? throw new InvalidOperationException("REPLY_EMAIL not found"),
                senderName: Environment.GetEnvironmentVariable("SENDER_NAME") ?? "CCAI.NET Test",
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