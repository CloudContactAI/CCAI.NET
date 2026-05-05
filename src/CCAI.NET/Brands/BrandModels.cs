// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Brands;

public class BrandRequest
{
    [JsonPropertyName("legalCompanyName")]
    public string? LegalCompanyName { get; set; }
    
    [JsonPropertyName("dba")]
    public string? Dba { get; set; }
    
    [JsonPropertyName("entityType")]
    public string? EntityType { get; set; }
    
    [JsonPropertyName("taxId")]
    public string? TaxId { get; set; }
    
    [JsonPropertyName("taxIdCountry")]
    public string? TaxIdCountry { get; set; }
    
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    
    [JsonPropertyName("verticalType")]
    public string? VerticalType { get; set; }
    
    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }
    
    [JsonPropertyName("stockSymbol")]
    public string? StockSymbol { get; set; }
    
    [JsonPropertyName("stockExchange")]
    public string? StockExchange { get; set; }
    
    [JsonPropertyName("street")]
    public string? Street { get; set; }
    
    [JsonPropertyName("city")]
    public string? City { get; set; }
    
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }
    
    [JsonPropertyName("contactFirstName")]
    public string? ContactFirstName { get; set; }
    
    [JsonPropertyName("contactLastName")]
    public string? ContactLastName { get; set; }
    
    [JsonPropertyName("contactEmail")]
    public string? ContactEmail { get; set; }
    
    [JsonPropertyName("contactPhone")]
    public string? ContactPhone { get; set; }
    
    [JsonPropertyName("websiteMatch")]
    public bool WebsiteMatch { get; set; } = false;
}

public class BrandResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("accountId")]
    public long AccountId { get; set; }
    
    [JsonPropertyName("legalCompanyName")]
    public string LegalCompanyName { get; set; } = "";
    
    [JsonPropertyName("dba")]
    public string? Dba { get; set; }
    
    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = "";
    
    [JsonPropertyName("taxId")]
    public string TaxId { get; set; } = "";
    
    [JsonPropertyName("taxIdCountry")]
    public string TaxIdCountry { get; set; } = "";
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = "";
    
    [JsonPropertyName("verticalType")]
    public string VerticalType { get; set; } = "";
    
    [JsonPropertyName("websiteUrl")]
    public string WebsiteUrl { get; set; } = "";
    
    [JsonPropertyName("stockSymbol")]
    public string? StockSymbol { get; set; }
    
    [JsonPropertyName("stockExchange")]
    public string? StockExchange { get; set; }
    
    [JsonPropertyName("street")]
    public string Street { get; set; } = "";
    
    [JsonPropertyName("city")]
    public string City { get; set; } = "";
    
    [JsonPropertyName("state")]
    public string State { get; set; } = "";
    
    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = "";
    
    [JsonPropertyName("contactFirstName")]
    public string ContactFirstName { get; set; } = "";
    
    [JsonPropertyName("contactLastName")]
    public string ContactLastName { get; set; } = "";
    
    [JsonPropertyName("contactEmail")]
    public string ContactEmail { get; set; } = "";
    
    [JsonPropertyName("contactPhone")]
    public string ContactPhone { get; set; } = "";
    
    [JsonPropertyName("websiteMatchScore")]
    public int? WebsiteMatchScore { get; set; }
    
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
    
    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";
}
