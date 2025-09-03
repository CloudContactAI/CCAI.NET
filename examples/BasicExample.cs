// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

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
        // Load environment variables
        Env.Load("./.env");
        
        // Create a new CCAI client
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ??
                      throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ??
                    throw new InvalidOperationException("CCAI_API_KEY not found"),
            UseTestEnvironment = true  // Automatically uses test URLs from env vars
        };
        
        using var ccai = new CCAIClient(config);
        
        // Load account data from environment variables
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = Environment.GetEnvironmentVariable("TEST_FIRST_NAME") ?? "John",
                LastName = Environment.GetEnvironmentVariable("TEST_LAST_NAME") ?? "Doe",
                Phone = Environment.GetEnvironmentVariable("TEST_PHONE_NUMBER") ?? "+14155551212"
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
        // Load environment variables
        Env.Load("./.env");
        
        // Create a new CCAI client
        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ??
                      throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ??
                    throw new InvalidOperationException("CCAI_API_KEY not found"),
            
        };
        
        using var ccai = new CCAIClient(config);
        
        // Load account data from environment variables
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = Environment.GetEnvironmentVariable("TEST_FIRST_NAME") ?? "John",
                LastName = Environment.GetEnvironmentVariable("TEST_LAST_NAME") ?? "Doe",
                Phone = Environment.GetEnvironmentVariable("TEST_PHONE_NUMBER") ?? "+14155551212"
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending SMS: {ex.Message}");
            throw;
        }
    }
}