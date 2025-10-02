// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CCAI.NET.Webhook;

/// <summary>
/// Event types supported by CloudContactAI webhooks
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WebhookEventType
{
    /// <summary>
    /// Triggered when a message is successfully sent from CloudContact to a recipient
    /// </summary>
    [EnumMember(Value = "message.sent")]
    MessageSent,
    
    /// <summary>
    /// Triggered when a recipient replies to one of your messages or sends a message to your CloudContact phone number
    /// </summary>
    [EnumMember(Value = "message.incoming")]
    MessageIncoming,
    
    /// <summary>
    /// Triggered when a message is excluded from being sent during campaign creation due to filtering rules
    /// </summary>
    [EnumMember(Value = "message.excluded")]
    MessageExcluded,
    
    /// <summary>
    /// Triggered when a message fails to be delivered due to carrier-level errors
    /// </summary>
    [EnumMember(Value = "message.error.carrier")]
    MessageErrorCarrier,
    
    /// <summary>
    /// Triggered when a message fails due to internal CloudContact system errors
    /// </summary>
    [EnumMember(Value = "message.error.cloudcontact")]
    MessageErrorCloudContact,
    
    /// <summary>
    /// Triggered when a contact is unsubscribed (flagged as "do not text")
    /// </summary>
    [EnumMember(Value = "contact.unsubscribed")]
    ContactUnsubscribed
}
