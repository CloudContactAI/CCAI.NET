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
    /// Triggered when a message is sent
    /// </summary>
    [EnumMember(Value = "message.sent")]
    MessageSent,
    
    /// <summary>
    /// Triggered when a message is received
    /// </summary>
    [EnumMember(Value = "message.received")]
    MessageReceived
}
