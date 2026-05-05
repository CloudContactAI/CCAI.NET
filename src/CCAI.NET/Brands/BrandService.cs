// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace CCAI.NET.Brands;

/// <summary>
/// Brand service for managing brand registrations
/// </summary>
public class BrandService
{
    private readonly CCAIClient _client;

    private static readonly HashSet<string> EntityTypes = new()
    {
        "PRIVATE_PROFIT", "PUBLIC_PROFIT", "NON_PROFIT", "GOVERNMENT", "SOLE_PROPRIETOR"
    };

    private static readonly HashSet<string> VerticalTypes = new()
    {
        "AUTOMOTIVE", "AGRICULTURE", "BANKING", "COMMUNICATION", "CONSTRUCTION", "EDUCATION",
        "ENERGY", "ENTERTAINMENT", "GOVERNMENT", "HEALTHCARE", "HOSPITALITY", "INSURANCE",
        "LEGAL", "MANUFACTURING", "NON_PROFIT", "PROFESSIONAL", "REAL_ESTATE", "RETAIL",
        "TECHNOLOGY", "TRANSPORTATION"
    };

    private static readonly HashSet<string> TaxIdCountries = new() { "US", "CA", "GB", "AU" };
    private static readonly HashSet<string> StockExchanges = new() { "NASDAQ", "NYSE", "AMEX", "TSX", "LON", "JPX", "HKEX", "OTHER" };

    public BrandService(CCAIClient client)
    {
        _client = client;
    }

    public async Task<BrandResponse> CreateAsync(BrandRequest data, CancellationToken cancellationToken = default)
    {
        Validate(data, isCreate: true);
        return await _client.CustomRequestAsync<BrandResponse>(HttpMethod.Post, "/v1/brands", data, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public BrandResponse Create(BrandRequest data) => CreateAsync(data).GetAwaiter().GetResult();

    public async Task<BrandResponse> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.CustomRequestAsync<BrandResponse>(HttpMethod.Get, $"/v1/brands/{id}", null, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public BrandResponse Get(long id) => GetAsync(id).GetAwaiter().GetResult();

    public async Task<BrandResponse[]> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _client.CustomRequestAsync<BrandResponse[]>(HttpMethod.Get, "/v1/brands", null, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public BrandResponse[] List() => ListAsync().GetAwaiter().GetResult();

    public async Task<BrandResponse> UpdateAsync(long id, BrandRequest data, CancellationToken cancellationToken = default)
    {
        Validate(data, isCreate: false);
        return await _client.CustomRequestAsync<BrandResponse>(HttpMethod.Patch, $"/v1/brands/{id}", data, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public BrandResponse Update(long id, BrandRequest data) => UpdateAsync(id, data).GetAwaiter().GetResult();

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.CustomRequestWithoutResponseAsync(HttpMethod.Delete, $"/v1/brands/{id}", null, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public void Delete(long id) => DeleteAsync(id).GetAwaiter().GetResult();

    private static void Validate(BrandRequest data, bool isCreate)
    {
        var errors = new List<string>();

        if (isCreate)
        {
            if (string.IsNullOrWhiteSpace(data.LegalCompanyName)) errors.Add("legalCompanyName is required");
            if (string.IsNullOrWhiteSpace(data.EntityType)) errors.Add("entityType is required");
            if (string.IsNullOrWhiteSpace(data.TaxId)) errors.Add("taxId is required");
            if (string.IsNullOrWhiteSpace(data.TaxIdCountry)) errors.Add("taxIdCountry is required");
            if (string.IsNullOrWhiteSpace(data.Country)) errors.Add("country is required");
            if (string.IsNullOrWhiteSpace(data.VerticalType)) errors.Add("verticalType is required");
            if (string.IsNullOrWhiteSpace(data.WebsiteUrl)) errors.Add("websiteUrl is required");
            if (string.IsNullOrWhiteSpace(data.Street)) errors.Add("street is required");
            if (string.IsNullOrWhiteSpace(data.City)) errors.Add("city is required");
            if (string.IsNullOrWhiteSpace(data.State)) errors.Add("state is required");
            if (string.IsNullOrWhiteSpace(data.PostalCode)) errors.Add("postalCode is required");
            if (string.IsNullOrWhiteSpace(data.ContactFirstName)) errors.Add("contactFirstName is required");
            if (string.IsNullOrWhiteSpace(data.ContactLastName)) errors.Add("contactLastName is required");
            if (string.IsNullOrWhiteSpace(data.ContactEmail)) errors.Add("contactEmail is required");
            if (string.IsNullOrWhiteSpace(data.ContactPhone)) errors.Add("contactPhone is required");
        }

        if (data.EntityType != null && !EntityTypes.Contains(data.EntityType)) errors.Add("Invalid entity type");
        if (data.VerticalType != null && !VerticalTypes.Contains(data.VerticalType)) errors.Add("Invalid vertical type");
        if (data.TaxIdCountry != null && !TaxIdCountries.Contains(data.TaxIdCountry)) errors.Add("Invalid tax ID country");
        if (data.StockExchange != null && !StockExchanges.Contains(data.StockExchange)) errors.Add("Invalid stock exchange");

        if (data.WebsiteUrl != null && !data.WebsiteUrl.StartsWith("http://") && !data.WebsiteUrl.StartsWith("https://"))
            errors.Add("Website URL must start with http:// or https://");

        if (data.ContactEmail != null && !Regex.IsMatch(data.ContactEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errors.Add("Invalid email format");

        if (data.TaxId != null && data.TaxIdCountry is "US" or "CA" && !Regex.IsMatch(data.TaxId, @"^\d{9}$"))
            errors.Add($"Tax ID must be exactly 9 digits for {data.TaxIdCountry}");

        if (data.EntityType == "PUBLIC_PROFIT")
        {
            if (string.IsNullOrWhiteSpace(data.StockSymbol)) errors.Add("Stock symbol is required for PUBLIC_PROFIT entities");
            if (string.IsNullOrWhiteSpace(data.StockExchange)) errors.Add("Stock exchange is required for PUBLIC_PROFIT entities");
        }

        if (errors.Count > 0)
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
    }
}
