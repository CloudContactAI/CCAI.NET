// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Webhook;

/// <summary>
/// Campaign information included in webhook events
/// </summary>
public record WebhookCampaign
{
    /// <summary>
    /// Campaign ID
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    /// <summary>
    /// Campaign title
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;
    
    /// <summary>
    /// Campaign message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Sender phone number
    /// </summary>
    [JsonPropertyName("senderPhone")]
    public string SenderPhone { get; init; } = string.Empty;
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; init; } = string.Empty;
    
    /// <summary>
    /// Execution timestamp
    /// </summary>
    [JsonPropertyName("runAt")]
    public string RunAt { get; init; } = string.Empty;
}

/// <summary>
/// Base class for all webhook events
/// </summary>
public abstract record WebhookEventBase
{
    /// <summary>
    /// Campaign information
    /// </summary>
    [JsonPropertyName("campaign")]
    public WebhookCampaign Campaign { get; init; } = new WebhookCampaign();
    
    /// <summary>
    /// Sender information
    /// </summary>
    [JsonPropertyName("from")]
    public string From { get; init; } = string.Empty;
    
    /// <summary>
    /// Recipient information
    /// </summary>
    [JsonPropertyName("to")]
    public string To { get; init; } = string.Empty;
    
    /// <summary>
    /// Message content
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Event type
    /// </summary>
    [JsonPropertyName("type")]
    public abstract WebhookEventType Type { get; }
}

/// <summary>
/// Message Sent (Outbound) webhook event
/// </summary>
public record MessageSentEvent : WebhookEventBase
{
    /// <summary>
    /// Event type (always MessageSent)
    /// </summary>
    [JsonPropertyName("type")]
    public override WebhookEventType Type => WebhookEventType.MessageSent;
}

/// <summary>
/// Message Received (Inbound) webhook event
/// </summary>
public record MessageReceivedEvent : WebhookEventBase
{
    /// <summary>
    /// Event type (always MessageReceived)
    /// </summary>
    [JsonPropertyName("type")]
    public override WebhookEventType Type => WebhookEventType.MessageReceived;
}
