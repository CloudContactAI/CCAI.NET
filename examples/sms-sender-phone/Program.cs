using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

namespace SmsSenderPhone;

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
            // Get list of available phones
            Console.WriteLine("Fetching available phones...");
            var phones = await ccai.Phone.ListAsync();
            
            if (phones == null || phones.Count == 0)
            {
                Console.WriteLine("No phones available for sending SMS");
                return;
            }
            
            // Use the first available phone
            var senderPhone = phones[0].PhoneNumber;
            Console.WriteLine($"Using sender phone: {senderPhone}");
            
            var recipientPhone = Environment.GetEnvironmentVariable("TEST_PHONE_NUMBER") ?? throw new InvalidOperationException("TEST_PHONE_NUMBER not found");
            var firstName = Environment.GetEnvironmentVariable("TEST_FIRST_NAME") ?? "John";
            var lastName = Environment.GetEnvironmentVariable("TEST_LAST_NAME") ?? "Doe";
            
            // Send first SMS message
            Console.WriteLine("Sending first SMS message...");
            var response1 = await ccai.SMS.SendSingleAsync(
                firstName: firstName,
                lastName: lastName,
                phone: recipientPhone,
                message: "Hello ${FirstName}! This is your first message from CCAI.NET at " + DateTime.Now.ToString("HH:mm:ss"),
                title: "First SMS Test",
                senderPhone: senderPhone
            );
            
            Console.WriteLine("First SMS sent successfully!");
            Console.WriteLine($"Status: {response1.Status}");
            
            // Wait a moment between messages
            await Task.Delay(2000);
            
            // Send second SMS message using the same sender phone
            Console.WriteLine("Sending second SMS message...");
            var response2 = await ccai.SMS.SendSingleAsync(
                firstName: firstName,
                lastName: lastName,
                phone: recipientPhone,
                message: "Hello ${FirstName}! This is your second message from the same phone at " + DateTime.Now.ToString("HH:mm:ss"),
                title: "Second SMS Test",
                senderPhone: senderPhone
            );
            
            Console.WriteLine("Second SMS sent successfully!");
            Console.WriteLine($"Status: {response2.Status}");
            Console.WriteLine($"Both messages sent using sender phone: {senderPhone}");
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
