// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Campaigns;
using DotNetEnv;

namespace CCAI.NET.Examples;

/// <summary>
/// Campaign registration example using the CCAI.NET client
/// </summary>
public class CampaignExample
{
    public static async Task RunAsync()
    {
        Env.Load("./.env");

        var config = new CCAIConfig
        {
            ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ??
                      throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
            ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ??
                    throw new InvalidOperationException("CCAI_API_KEY not found"),
            UseTestEnvironment = true
        };

        using var ccai = new CCAIClient(config);

        try
        {
            // Create a campaign (assumes brand ID 1 exists)
            Console.WriteLine("Creating a campaign...");
            var campaign = await ccai.Campaigns.CreateAsync(new CampaignRequest
            {
                BrandId = 1,
                UseCase = "MIXED",
                SubUseCases = new List<string> { "CUSTOMER_CARE", "TWO_FACTOR_AUTHENTICATION", "ACCOUNT_NOTIFICATION" },
                Description = "This campaign handles security codes and support for Collect.org.",
                MessageFlow = "Users opt-in via our signup form checkbox at https://collect.org/signup",
                TermsLink = "https://collect.org/terms",
                PrivacyLink = "https://collect.org/privacy",
                HasEmbeddedLinks = true,
                HasEmbeddedPhone = false,
                IsAgeGated = false,
                IsDirectLending = false,
                OptInKeywords = new List<string> { "START", "JOIN" },
                OptInMessage = "Welcome to Collect.org! Msg&Data rates may apply. Reply STOP to cancel.",
                OptInProofUrl = "https://collect.org/images/opt-in-proof.png",
                HelpKeywords = new List<string> { "HELP", "INFO" },
                HelpMessage = "Collect.org: For help email support@collect.org. Reply STOP to cancel.",
                OptOutKeywords = new List<string> { "STOP", "UNSUBSCRIBE" },
                OptOutMessage = "Collect.org: You have been unsubscribed. STOP received.",
                SampleMessages = new List<string>
                {
                    "Your Collect.org security code is 554321. Reply STOP to cancel.",
                    "Hi [Name], your ticket #[ID] has been updated. Reply HELP for more info."
                }
            });
            Console.WriteLine($"Campaign created with ID: {campaign.Id}, fee: ${campaign.MonthlyFee}/mo");

            // Get campaign by ID
            Console.WriteLine("\nFetching campaign by ID...");
            var fetched = await ccai.Campaigns.GetAsync(campaign.Id);
            Console.WriteLine($"Campaign: {fetched.UseCase}, Brand: {fetched.BrandId}");

            // List all campaigns
            Console.WriteLine("\nListing all campaigns...");
            var campaigns = await ccai.Campaigns.ListAsync();
            Console.WriteLine($"Found {campaigns.Length} campaign(s)");

            // Update a campaign
            Console.WriteLine("\nUpdating campaign...");
            var updated = await ccai.Campaigns.UpdateAsync(campaign.Id, new CampaignRequest
            {
                Description = "Updated campaign description for Collect.org messaging.",
                SampleMessages = new List<string>
                {
                    "Your Collect.org code is 123456. Reply STOP to opt-out.",
                    "Your support ticket has been resolved. Reply HELP for more info.",
                    "Your payment of $50.00 was received. Reply STOP to cancel."
                }
            });
            Console.WriteLine($"Campaign updated: {updated.Description}");

            // Delete a campaign
            Console.WriteLine("\nDeleting campaign...");
            await ccai.Campaigns.DeleteAsync(campaign.Id);
            Console.WriteLine("Campaign deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}
