using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

namespace SmsSender;

public class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();
        
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
        };
        
        using var ccai = new CCAIClient(config);
        
        try
        {
            Console.WriteLine("Sending SMS...");
            var response = await ccai.SMS.SendSingleAsync(
                firstName: Environment.GetEnvironmentVariable("TEST_FIRST_NAME") ?? "John",
                lastName: Environment.GetEnvironmentVariable("TEST_LAST_NAME") ?? "Doe",
                phone: Environment.GetEnvironmentVariable("TEST_PHONE_NUMBER") ?? throw new InvalidOperationException("TEST_PHONE_NUMBER not found"),
                message: "Hello ${FirstName}! This is a webhook test message from your CCAI.NET library at " + DateTime.Now.ToString("HH:mm:ss"),
                title: "Webhook Test"
            );
            
            Console.WriteLine("SMS sent successfully!");
            Console.WriteLine($"Status: {response.Status}");
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