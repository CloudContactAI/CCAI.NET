// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Webhook;

/// <summary>
/// CloudContact webhook event wrapper
/// </summary>
public record CloudContactWebhookEvent
{
    /// <summary>
    /// Event type identifier
    /// </summary>
    [JsonPropertyName("eventType")]
    public string EventType { get; init; } = string.Empty;
    
    /// <summary>
    /// Event data containing the message information
    /// </summary>
    [JsonPropertyName("data")]
    public WebhookEventData Data { get; init; } = new();
}

/// <summary>
/// Webhook event data containing message information
/// </summary>
public record WebhookEventData
{
    /// <summary>
    /// SMS ID from CloudContact
    /// </summary>
    [JsonPropertyName("SmsSid")]
    public int SmsSid { get; init; }
    
    /// <summary>
    /// Message status (DELIVERED, RECEIVED, EXCLUDED, FAILED)
    /// </summary>
    [JsonPropertyName("MessageStatus")]
    public string MessageStatus { get; init; } = string.Empty;
    
    /// <summary>
    /// Recipient phone number
    /// </summary>
    [JsonPropertyName("To")]
    public string To { get; init; } = string.Empty;
    
    /// <summary>
    /// Message content
    /// </summary>
    [JsonPropertyName("Message")]
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Custom data associated with the message
    /// </summary>
    [JsonPropertyName("CustomData")]
    public string CustomData { get; init; } = string.Empty;
    
    /// <summary>
    /// Client external ID for tracking
    /// </summary>
    [JsonPropertyName("ClientExternalId")]
    public string ClientExternalId { get; init; } = string.Empty;
    
    /// <summary>
    /// Campaign ID (0 for non-campaign messages)
    /// </summary>
    [JsonPropertyName("CampaignId")]
    public int CampaignId { get; init; }
    
    /// <summary>
    /// Campaign title
    /// </summary>
    [JsonPropertyName("CampaignTitle")]
    public string CampaignTitle { get; init; } = string.Empty;
    
    /// <summary>
    /// Sender phone number (for incoming messages)
    /// </summary>
    [JsonPropertyName("From")]
    public string? From { get; init; }
    
    /// <summary>
    /// Number of message segments (for sent messages)
    /// </summary>
    [JsonPropertyName("Segments")]
    public int? Segments { get; init; }
    
    /// <summary>
    /// Total price for the message (for sent messages)
    /// </summary>
    [JsonPropertyName("TotalPrice")]
    public decimal? TotalPrice { get; init; }
    
    /// <summary>
    /// Reason for exclusion (for excluded messages)
    /// </summary>
    [JsonPropertyName("ExcludedReason")]
    public string? ExcludedReason { get; init; }
    
    /// <summary>
    /// Error code (for error events)
    /// </summary>
    [JsonPropertyName("ErrorCode")]
    public string? ErrorCode { get; init; }
    
    /// <summary>
    /// Error message (for error events)
    /// </summary>
    [JsonPropertyName("ErrorMessage")]
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Error type (carrier or cloudcontact)
    /// </summary>
    [JsonPropertyName("ErrorType")]
    public string? ErrorType { get; init; }
    
    /// <summary>
    /// Timestamp when contact was unsubscribed (for contact.unsubscribed events)
    /// </summary>
    [JsonPropertyName("UnsubscribedAt")]
    public string? UnsubscribedAt { get; init; }
    
    /// <summary>
    /// Contact information (for contact.unsubscribed events)
    /// </summary>
    [JsonPropertyName("ContactData")]
    public ContactData? ContactData { get; init; }
}

/// <summary>
/// Contact information for unsubscribe events
/// </summary>
public record ContactData
{
    /// <summary>
    /// Contact's first name
    /// </summary>
    [JsonPropertyName("firstName")]
    public string? FirstName { get; init; }
    
    /// <summary>
    /// Contact's last name
    /// </summary>
    [JsonPropertyName("lastName")]
    public string? LastName { get; init; }
    
    /// <summary>
    /// Contact's email address
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }
    
    /// <summary>
    /// Contact's phone number
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; init; }
}

/// <summary>
/// Base class for all webhook events (legacy support)
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
/// Campaign information included in webhook events (legacy support)
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
/// Message Sent webhook event (legacy support)
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
/// Message Incoming webhook event (legacy support)
/// </summary>
public record MessageIncomingEvent : WebhookEventBase
{
    /// <summary>
    /// Event type (always MessageIncoming)
    /// </summary>
    [JsonPropertyName("type")]
    public override WebhookEventType Type => WebhookEventType.MessageIncoming;
}

/// <summary>
/// Contact Unsubscribed webhook event (legacy support)
/// </summary>
public record ContactUnsubscribedEvent : WebhookEventBase
{
    /// <summary>
    /// Event type (always ContactUnsubscribed)
    /// </summary>
    [JsonPropertyName("type")]
    public override WebhookEventType Type => WebhookEventType.ContactUnsubscribed;
    
    /// <summary>
    /// Timestamp when contact was unsubscribed
    /// </summary>
    [JsonPropertyName("unsubscribedAt")]
    public string UnsubscribedAt { get; init; } = string.Empty;
    
    /// <summary>
    /// Contact information
    /// </summary>
    [JsonPropertyName("contactData")]
    public ContactData ContactData { get; init; } = new();
}
