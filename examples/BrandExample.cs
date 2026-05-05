// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.Brands;
using DotNetEnv;

namespace CCAI.NET.Examples;

/// <summary>
/// Brand registration example using the CCAI.NET client
/// </summary>
public class BrandExample
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
            // Create a brand
            Console.WriteLine("Creating a brand...");
            var brand = await ccai.Brands.CreateAsync(new BrandRequest
            {
                LegalCompanyName = "Collect.org Inc.",
                Dba = "Collect",
                EntityType = "NON_PROFIT",
                TaxId = "123456789",
                TaxIdCountry = "US",
                Country = "US",
                VerticalType = "NON_PROFIT",
                WebsiteUrl = "https://www.collect.org",
                Street = "123 Main Street",
                City = "San Francisco",
                State = "CA",
                PostalCode = "94105",
                ContactFirstName = "Jane",
                ContactLastName = "Doe",
                ContactEmail = "jane@collect.org",
                ContactPhone = "+14155551234"
            });
            Console.WriteLine($"Brand created with ID: {brand.Id}");

            // Get brand by ID
            Console.WriteLine("\nFetching brand by ID...");
            var fetched = await ccai.Brands.GetAsync(brand.Id);
            Console.WriteLine($"Brand: {fetched.LegalCompanyName}, Score: {fetched.WebsiteMatchScore?.ToString() ?? "pending"}");

            // List all brands
            Console.WriteLine("\nListing all brands...");
            var brands = await ccai.Brands.ListAsync();
            Console.WriteLine($"Found {brands.Length} brand(s)");

            // Update a brand
            Console.WriteLine("\nUpdating brand...");
            var updated = await ccai.Brands.UpdateAsync(brand.Id, new BrandRequest
            {
                Street = "456 Oak Avenue",
                City = "Los Angeles",
                ContactEmail = "admin@collect.org"
            });
            Console.WriteLine($"Brand updated: {updated.Street}, {updated.City}");

            // Delete a brand
            Console.WriteLine("\nDeleting brand...");
            await ccai.Brands.DeleteAsync(brand.Id);
            Console.WriteLine("Brand deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}
