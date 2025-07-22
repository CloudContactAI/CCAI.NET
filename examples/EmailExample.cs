// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CCAI.NET;
using CCAI.NET.Email;

namespace Examples;

/// <summary>
/// Example of using the Email functionality
/// </summary>
public class EmailExample
{
    /// <summary>
    /// Run the example
    /// </summary>
    public static async Task Run()
    {
        Console.WriteLine("Email Example");
        Console.WriteLine("------------");
        
        // Create a CCAI client
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "YOUR_CLIENT_ID", // Replace with your client ID
            ApiKey = "YOUR_API_KEY" // Replace with your API key
        });
        
        try
        {
            // Example 1: Send a single email
            await SendSingleEmail(client);
            
            // Example 2: Send an email campaign to multiple recipients
            await SendEmailCampaign(client);
            
            // Example 3: Schedule an email campaign for future delivery
            await ScheduleEmailCampaign(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 1: Send a single email
    /// </summary>
    private static async Task SendSingleEmail(CCAIClient client)
    {
        Console.WriteLine("\nSending a single email...");
        
        try
        {
            var response = await client.Email.SendSingleAsync(
                firstName: "John",
                lastName: "Doe",
                email: "john@example.com", // Replace with a real email for testing
                subject: "Welcome to Our Service",
                message: "<p>Hello John,</p><p>Thank you for signing up for our service!</p><p>Best regards,<br>The Team</p>",
                senderEmail: "noreply@yourcompany.com", // Replace with your sender email
                replyEmail: "support@yourcompany.com", // Replace with your reply-to email
                senderName: "Your Company",
                title: "Welcome Email",
                options: new EmailOptions
                {
                    OnProgress = status => Console.WriteLine($"Status: {status}")
                }
            );
            
            Console.WriteLine($"Email sent successfully: ID={response.Id}, Status={response.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 2: Send an email campaign to multiple recipients
    /// </summary>
    private static async Task SendEmailCampaign(CCAIClient client)
    {
        Console.WriteLine("\nSending an email campaign to multiple recipients...");
        
        try
        {
            var campaign = new EmailCampaign
            {
                Subject = "Monthly Newsletter",
                Title = "July 2025 Newsletter",
                Message = @"
                    <h1>Monthly Newsletter - July 2025</h1>
                    <p>Hello ${FirstName},</p>
                    <p>Here are our updates for this month:</p>
                    <ul>
                        <li>New feature: Email campaigns</li>
                        <li>Improved performance</li>
                        <li>Bug fixes</li>
                    </ul>
                    <p>Thank you for being a valued customer!</p>
                    <p>Best regards,<br>The Team</p>
                ",
                SenderEmail = "newsletter@yourcompany.com", // Replace with your sender email
                ReplyEmail = "support@yourcompany.com", // Replace with your reply-to email
                SenderName = "Your Company Newsletter",
                Accounts = new List<EmailAccount>
                {
                    new EmailAccount
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john@example.com" // Replace with a real email for testing
                    },
                    new EmailAccount
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane@example.com" // Replace with a real email for testing
                    }
                }
            };
            
            var response = await client.Email.SendCampaignAsync(
                campaign,
                new EmailOptions
                {
                    OnProgress = status => Console.WriteLine($"Status: {status}")
                }
            );
            
            Console.WriteLine($"Email campaign sent successfully: ID={response.Id}, Status={response.Status}, Messages Sent={response.MessagesSent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email campaign: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 3: Schedule an email campaign for future delivery
    /// </summary>
    private static async Task ScheduleEmailCampaign(CCAIClient client)
    {
        Console.WriteLine("\nScheduling an email campaign for future delivery...");
        
        try
        {
            // Schedule for tomorrow at 10:00 AM
            var tomorrow = DateTime.Now.AddDays(1).Date.AddHours(10);
            
            var campaign = new EmailCampaign
            {
                Subject = "Upcoming Event Reminder",
                Title = "Event Reminder Campaign",
                Message = @"
                    <h1>Reminder: Upcoming Event</h1>
                    <p>Hello ${FirstName},</p>
                    <p>This is a reminder about our upcoming event tomorrow at 2:00 PM.</p>
                    <p>We look forward to seeing you there!</p>
                    <p>Best regards,<br>The Events Team</p>
                ",
                SenderEmail = "events@yourcompany.com", // Replace with your sender email
                ReplyEmail = "events@yourcompany.com", // Replace with your reply-to email
                SenderName = "Your Company Events",
                Accounts = new List<EmailAccount>
                {
                    new EmailAccount
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john@example.com" // Replace with a real email for testing
                    }
                },
                ScheduledTimestamp = tomorrow.ToString("o"), // ISO 8601 format
                ScheduledTimezone = "America/New_York"
            };
            
            var response = await client.Email.SendCampaignAsync(campaign);
            
            Console.WriteLine($"Email campaign scheduled successfully: ID={response.Id}, Status={response.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to schedule email campaign: {ex.Message}");
        }
    }
}
