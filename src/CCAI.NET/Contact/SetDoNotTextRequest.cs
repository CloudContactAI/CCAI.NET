// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Contact;

/// <summary>
/// Request to set the do-not-text preference for a contact
/// </summary>
public class SetDoNotTextRequest
{
    /// <summary>
    /// Whether to opt the contact out of SMS messages
    /// </summary>
    [JsonPropertyName("doNotText")]
    public bool DoNotText { get; set; }

    /// <summary>
    /// Optional contact ID
    /// </summary>
    [JsonPropertyName("contactId")]
    public string? ContactId { get; set; }

    /// <summary>
    /// Optional phone number in E.164 format
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>
    /// Client ID — injected automatically by ContactService
    /// </summary>
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;
}
