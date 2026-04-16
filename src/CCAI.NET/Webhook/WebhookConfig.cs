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
    /// Optional secret key for webhook signature verification
    /// </summary>
    [JsonPropertyName("secretKey")]
    public string? Secret { get; init; }

    /// <summary>
    /// Integration type filter (default: "ALL")
    /// </summary>
    [JsonPropertyName("integrationType")]
    public string IntegrationType { get; init; } = "ALL";
}
