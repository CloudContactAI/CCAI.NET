// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;

namespace CCAI.NET.Examples;

/// <summary>
/// Basic example using the CCAI.NET client
/// </summary>
public class BasicExample
{
    /// <summary>
    /// Run the basic example
    /// </summary>
    public static async Task RunAsync()
    {
        // Create a new CCAI client
        var config = new CCAIConfig
        {
            ClientId = "2682",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
        };
        
        using var ccai = new CCAIClient(config);
        
        // Example recipients
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+14156961732"  // Use E.164 format
            }
        };
        
        // Message with variable placeholders
        var message = "Hello ${FirstName} ${LastName}, this is a test message!";
        var title = "Test Campaign";
        
        try
        {
            // Method 1: Send SMS to multiple recipients
            Console.WriteLine("Sending campaign to multiple recipients...");
            var campaignResponse = await ccai.SMS.SendAsync(
                accounts: accounts,
                message: message,
                title: title
            );
            Console.WriteLine("SMS campaign sent successfully!");
            Console.WriteLine($"ID: {campaignResponse.Id}");
            Console.WriteLine($"Status: {campaignResponse.Status}");
            Console.WriteLine($"Campaign ID: {campaignResponse.CampaignId}");
            Console.WriteLine($"Messages sent: {campaignResponse.MessagesSent}");
            
            // Method 2: Send SMS to a single recipient
            Console.WriteLine("\nSending message to a single recipient...");
            var singleResponse = await ccai.SMS.SendSingleAsync(
                firstName: "Jane",
                lastName: "Smith",
                phone: "+14152440933",
                message: "Hi ${FirstName}, thanks for your interest!",
                title: "Single Message Test"
            );
            Console.WriteLine("Single SMS sent successfully!");
            Console.WriteLine($"ID: {singleResponse.Id}");
            Console.WriteLine($"Status: {singleResponse.Status}");
            
            Console.WriteLine("\nAll messages sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending SMS: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Run the basic example synchronously
    /// </summary>
    public static void Run()
    {
        // Create a new CCAI client
        var config = new CCAIConfig
        {
            ClientId = "2682",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
        };
        
        using var ccai = new CCAIClient(config);
        
        // Example recipients
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+14156961732"  // Use E.164 format
            }
        };
        
        // Message with variable placeholders
        var message = "Hello ${FirstName} ${LastName}, this is a test message!";
        var title = "Test Campaign";
        
        try
        {
            // Method 1: Send SMS to multiple recipients
            Console.WriteLine("Sending campaign to multiple recipients...");
            var campaignResponse = ccai.SMS.Send(
                accounts: accounts,
                message: message,
                title: title
            );
            Console.WriteLine("SMS campaign sent successfully!");
            Console.WriteLine($"ID: {campaignResponse.Id}");
            Console.WriteLine($"Status: {campaignResponse.Status}");
            
            // Method 2: Send SMS to a single recipient
            Console.WriteLine("\nSending message to a single recipient...");
            var singleResponse = ccai.SMS.SendSingle(
                firstName: "Jane",
                lastName: "Smith",
                phone: "+14152440933",
                message: "Hi ${FirstName}, thanks for your interest!",
                title: "Single Message Test"
            );
            Console.WriteLine("Single SMS sent successfully!");
            Console.WriteLine($"ID: {singleResponse.Id}");
            Console.WriteLine($"Status: {singleResponse.Status}");
            
            Console.WriteLine("\nAll messages sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending SMS: {ex.Message}");
            throw;
        }
    }
}