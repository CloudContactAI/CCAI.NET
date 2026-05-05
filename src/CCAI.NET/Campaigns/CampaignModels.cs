// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Campaigns;

public class CampaignRequest
{
    [JsonPropertyName("brandId")]
    public long? BrandId { get; set; }
    
    [JsonPropertyName("useCase")]
    public string? UseCase { get; set; }
    
    [JsonPropertyName("subUseCases")]
    public List<string>? SubUseCases { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("messageFlow")]
    public string? MessageFlow { get; set; }
    
    [JsonPropertyName("termsLink")]
    public string? TermsLink { get; set; }
    
    [JsonPropertyName("privacyLink")]
    public string? PrivacyLink { get; set; }
    
    [JsonPropertyName("hasEmbeddedLinks")]
    public bool? HasEmbeddedLinks { get; set; }
    
    [JsonPropertyName("hasEmbeddedPhone")]
    public bool? HasEmbeddedPhone { get; set; }
    
    [JsonPropertyName("isAgeGated")]
    public bool? IsAgeGated { get; set; }
    
    [JsonPropertyName("isDirectLending")]
    public bool? IsDirectLending { get; set; }
    
    [JsonPropertyName("optInKeywords")]
    public List<string>? OptInKeywords { get; set; }
    
    [JsonPropertyName("optInMessage")]
    public string? OptInMessage { get; set; }
    
    [JsonPropertyName("optInProofUrl")]
    public string? OptInProofUrl { get; set; }
    
    [JsonPropertyName("helpKeywords")]
    public List<string>? HelpKeywords { get; set; }
    
    [JsonPropertyName("helpMessage")]
    public string? HelpMessage { get; set; }
    
    [JsonPropertyName("optOutKeywords")]
    public List<string>? OptOutKeywords { get; set; }
    
    [JsonPropertyName("optOutMessage")]
    public string? OptOutMessage { get; set; }
    
    [JsonPropertyName("sampleMessages")]
    public List<string>? SampleMessages { get; set; }
}

public class CampaignResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("accountId")]
    public long AccountId { get; set; }
    
    [JsonPropertyName("brandId")]
    public long BrandId { get; set; }
    
    [JsonPropertyName("useCase")]
    public string UseCase { get; set; } = "";
    
    [JsonPropertyName("subUseCases")]
    public List<string> SubUseCases { get; set; } = new();
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    
    [JsonPropertyName("messageFlow")]
    public string MessageFlow { get; set; } = "";
    
    [JsonPropertyName("termsLink")]
    public string? TermsLink { get; set; }
    
    [JsonPropertyName("privacyLink")]
    public string? PrivacyLink { get; set; }
    
    [JsonPropertyName("hasEmbeddedLinks")]
    public bool HasEmbeddedLinks { get; set; }
    
    [JsonPropertyName("hasEmbeddedPhone")]
    public bool HasEmbeddedPhone { get; set; }
    
    [JsonPropertyName("isAgeGated")]
    public bool IsAgeGated { get; set; }
    
    [JsonPropertyName("isDirectLending")]
    public bool IsDirectLending { get; set; }
    
    [JsonPropertyName("optInKeywords")]
    public List<string> OptInKeywords { get; set; } = new();
    
    [JsonPropertyName("optInMessage")]
    public string OptInMessage { get; set; } = "";
    
    [JsonPropertyName("optInProofUrl")]
    public string OptInProofUrl { get; set; } = "";
    
    [JsonPropertyName("helpKeywords")]
    public List<string> HelpKeywords { get; set; } = new();
    
    [JsonPropertyName("helpMessage")]
    public string HelpMessage { get; set; } = "";
    
    [JsonPropertyName("optOutKeywords")]
    public List<string> OptOutKeywords { get; set; } = new();
    
    [JsonPropertyName("optOutMessage")]
    public string OptOutMessage { get; set; } = "";
    
    [JsonPropertyName("sampleMessages")]
    public List<string> SampleMessages { get; set; } = new();
    
    [JsonPropertyName("monthlyFee")]
    public decimal MonthlyFee { get; set; }
    
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
    
    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";
}
