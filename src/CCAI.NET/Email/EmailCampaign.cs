// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.Email;

/// <summary>
/// Email campaign configuration
/// </summary>
public record EmailCampaign
{
    /// <summary>
    /// Email subject
    /// </summary>
    [JsonPropertyName("subject")]
    public required string Subject { get; init; }
    
    /// <summary>
    /// Campaign title
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    
    /// <summary>
    /// HTML message content
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
    
    /// <summary>
    /// Optional editor information
    /// </summary>
    [JsonPropertyName("editor")]
    public string? Editor { get; init; }
    
    /// <summary>
    /// Optional file key
    /// </summary>
    [JsonPropertyName("fileKey")]
    public string? FileKey { get; init; }
    
    /// <summary>
    /// Sender's email address
    /// </summary>
    [JsonPropertyName("senderEmail")]
    public required string SenderEmail { get; init; }
    
    /// <summary>
    /// Reply-to email address
    /// </summary>
    [JsonPropertyName("replyEmail")]
    public required string ReplyEmail { get; init; }
    
    /// <summary>
    /// Sender's name
    /// </summary>
    [JsonPropertyName("senderName")]
    public required string SenderName { get; init; }
    
    /// <summary>
    /// Recipients
    /// </summary>
    [JsonPropertyName("accounts")]
    public required IList<EmailAccount> Accounts { get; init; }
    
    /// <summary>
    /// Campaign type (must be "EMAIL")
    /// </summary>
    [JsonPropertyName("campaignType")]
    public string CampaignType { get; init; } = "EMAIL";
    
    /// <summary>
    /// Optional scheduled timestamp (ISO format)
    /// </summary>
    [JsonPropertyName("scheduledTimestamp")]
    public string? ScheduledTimestamp { get; init; }
    
    /// <summary>
    /// Optional timezone for scheduling
    /// </summary>
    [JsonPropertyName("scheduledTimezone")]
    public string? ScheduledTimezone { get; init; }
    
    /// <summary>
    /// List handling ("noList" or other options)
    /// </summary>
    [JsonPropertyName("addToList")]
    public string AddToList { get; init; } = "noList";
    
    /// <summary>
    /// Optional selected list
    /// </summary>
    [JsonPropertyName("selectedList")]
    public SelectedList? SelectedList { get; init; }
    
    /// <summary>
    /// Optional list ID
    /// </summary>
    [JsonPropertyName("listId")]
    public string? ListId { get; init; }
    
    /// <summary>
    /// How contacts are provided ("accounts")
    /// </summary>
    [JsonPropertyName("contactInput")]
    public string ContactInput { get; init; } = "accounts";
    
    /// <summary>
    /// Optional replace contacts flag
    /// </summary>
    [JsonPropertyName("replaceContacts")]
    public bool? ReplaceContacts { get; init; }
    
    /// <summary>
    /// Optional email template ID
    /// </summary>
    [JsonPropertyName("emailTemplateId")]
    public string? EmailTemplateId { get; init; }
    
    /// <summary>
    /// Optional flux ID
    /// </summary>
    [JsonPropertyName("fluxId")]
    public string? FluxId { get; init; }
    
    /// <summary>
    /// From type ("single" or other options)
    /// </summary>
    [JsonPropertyName("fromType")]
    public string FromType { get; init; } = "single";
    
    /// <summary>
    /// Additional senders (usually empty)
    /// </summary>
    [JsonPropertyName("senders")]
    public IList<object> Senders { get; init; } = new List<object>();
}

/// <summary>
/// Selected list model
/// </summary>
public record SelectedList
{
    /// <summary>
    /// List value
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}
