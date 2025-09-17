// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// Phone model representing a client phone
/// </summary>
public record Phone
{
    /// <summary>
    /// Phone ID
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    public required string PhoneNumber { get; init; }
    
    /// <summary>
    /// Phone label
    /// </summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; init; }
}
