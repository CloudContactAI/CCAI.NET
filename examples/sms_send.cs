using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

namespace CCAI.NET.Examples;

public class SmsSend
{
    public static async Task RunAsync()
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
            Console.WriteLine("Sending SMS...");
            var response = await ccai.SMS.SendSingleAsync(
                firstName: Environment.GetEnvironmentVariable("TEST_FIRST_NAME") ?? "John",
                lastName: Environment.GetEnvironmentVariable("TEST_LAST_NAME") ?? "Doe",
                phone: Environment.GetEnvironmentVariable("TEST_PHONE_NUMBER") ?? throw new InvalidOperationException("TEST_PHONE_NUMBER not found"),
                message: "Hello ${FirstName}! We are testing the CCAI SMS functionality with the webhooks",
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