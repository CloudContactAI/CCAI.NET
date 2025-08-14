using CCAI.NET.Examples;

namespace CCAI.NET.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "webhook")
            {
                await RegisterWebhook.RunAsync();
            }
            else if (args.Length > 0 && args[0].StartsWith("https://") && args[0].Contains("ngrok"))
            {
                // Test webhook with ngrok URL
                await TestWebhook.RunAsync(args[0]);
            }
            else if (args.Length > 0 && args[0] == "test-webhook")
            {
                Console.WriteLine("Please provide your ngrok URL:");
                Console.WriteLine("Usage: dotnet run https://your-ngrok-url.ngrok.io");
            }
            else
            {
                await SmsSend.RunAsync();
            }
        }
    }
}
