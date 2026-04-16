// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.Contact;

/// <summary>
/// Response from the do-not-text API
/// </summary>
public class ContactResponse
{
    /// <summary>
    /// Whether the contact is opted out of SMS messages
    /// </summary>
    [JsonPropertyName("doNotText")]
    public bool DoNotText { get; set; }

    /// <summary>
    /// Phone number of the contact
    /// </summary>
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Contact ID
    /// </summary>
    [JsonPropertyName("contactId")]
    public string? ContactId { get; set; }

    /// <summary>
    /// Additional fields from the API response
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
