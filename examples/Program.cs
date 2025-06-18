// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.Examples;

/// <summary>
/// Main program for running examples
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point
    /// </summary>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("CCAI.NET Examples");
        Console.WriteLine("================");
        
        // Uncomment one of the examples to run it
        
        // Run the basic example asynchronously
        // await BasicExample.RunAsync();
        
        // Run the basic example synchronously
        // BasicExample.Run();
        
        // Run the progress tracking example
        // await ProgressTrackingExample.RunAsync();
        
        // Run the MMS example
        // var mmsExample = new MMSExample("YOUR-CLIENT-ID", "YOUR-API-KEY");
        // await mmsExample.RunAllExamplesAsync();
        
        Console.WriteLine("\nExamples completed. Press any key to exit.");
        Console.ReadKey();
    }
}
