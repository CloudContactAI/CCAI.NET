// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;
using CCAI.NET.SMS;

namespace CCAI.NET.Email;

/// <summary>
/// Response from the email campaign API
/// </summary>
public record EmailResponse
{
    /// <summary>
    /// Campaign ID
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleIdConverter))]
    public string? Id { get; init; }
    
    /// <summary>
    /// Campaign status
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }
    
    /// <summary>
    /// Campaign ID (alternative property)
    /// </summary>
    [JsonPropertyName("campaignId")]
    [JsonConverter(typeof(FlexibleIdConverter))]
    public string? CampaignId { get; init; }
    
    /// <summary>
    /// Number of messages sent
    /// </summary>
    [JsonPropertyName("messagesSent")]
    public int? MessagesSent { get; init; }
    
    /// <summary>
    /// Timestamp of the campaign
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; init; }
    
    /// <summary>
    /// Additional properties returned by the API
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? AdditionalProperties { get; init; }
}
