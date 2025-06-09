// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// SMS campaign data model
/// </summary>
public record SMSCampaign
{
    /// <summary>
    /// List of recipient accounts
    /// </summary>
    [JsonPropertyName("accounts")]
    public required IEnumerable<Account> Accounts { get; init; }
    
    /// <summary>
    /// Message content with optional variables
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
    
    /// <summary>
    /// Campaign title
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }
}
