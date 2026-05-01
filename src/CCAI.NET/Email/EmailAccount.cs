// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Email;

/// <summary>
/// Account model representing an email recipient
/// </summary>
public record EmailAccount
{
    /// <summary>
    /// Recipient's first name
    /// </summary>
    [JsonPropertyName("firstName")]
    public required string FirstName { get; init; }
    
    /// <summary>
    /// Recipient's last name
    /// </summary>
    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }
    
    /// <summary>
    /// Recipient's email address
    /// </summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }
    
    /// <summary>
    /// Recipient's phone number (not used for email but required by API)
    /// </summary>
    [JsonPropertyName("phone")]
    public string Phone { get; init; } = string.Empty;
    
    /// <summary>
    /// Custom ID provided for this recipient. This can be used to link this account to an external system
    /// </summary>
    [JsonPropertyName("customAccountId")]
    public string? CustomAccountId { get; init; } = null;

    /// <summary>
    /// Additional key-value pairs for variable substitution in email templates.
    /// Define any keys you want and use them as ${key} in your message.
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, string>? Data { get; init; } = null;
}
