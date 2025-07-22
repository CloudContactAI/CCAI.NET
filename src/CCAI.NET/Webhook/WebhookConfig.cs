// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Webhook;

/// <summary>
/// Configuration for webhook integration
/// </summary>
public record WebhookConfig
{
    /// <summary>
    /// URL to receive webhook events
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }
    
    /// <summary>
    /// Events to subscribe to
    /// </summary>
    [JsonPropertyName("events")]
    public required IList<WebhookEventType> Events { get; init; }
    
    /// <summary>
    /// Optional secret for webhook signature verification
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; init; }
}
