// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// Account model representing a recipient
/// </summary>
public record Account
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
    /// Recipient's phone number in E.164 format
    /// </summary>
    [JsonPropertyName("phone")]
    public required string Phone { get; init; }
}
