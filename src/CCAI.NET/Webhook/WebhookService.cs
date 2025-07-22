// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.Webhook;

/// <summary>
/// Service for managing CloudContactAI webhooks
/// </summary>
public class WebhookService
{
    private readonly CCAIClient _client;
    
    /// <summary>
    /// Create a new Webhook service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public WebhookService(CCAIClient client)
    {
        _client = client;
    }
    
    /// <summary>
    /// Register a new webhook endpoint
    /// </summary>
    /// <param name="config">Webhook configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registered webhook details</returns>
    public async Task<WebhookRegistrationResponse> RegisterAsync(
        WebhookConfig config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(config.Url))
        {
            throw new ArgumentException("URL is required", nameof(config.Url));
        }
        
        if (config.Events == null || config.Events.Count == 0)
        {
            throw new ArgumentException("At least one event type is required", nameof(config.Events));
        }
        
        return await _client.RequestAsync<WebhookRegistrationResponse>(
            HttpMethod.Post,
            "/webhooks",
            config,
            cancellationToken);
    }
    
    /// <summary>
    /// Update an existing webhook configuration
    /// </summary>
    /// <param name="id">Webhook ID</param>
    /// <param name="config">Updated webhook configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated webhook details</returns>
    public async Task<WebhookRegistrationResponse> UpdateAsync(
        string id,
        WebhookConfig config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Webhook ID is required", nameof(id));
        }
        
        return await _client.RequestAsync<WebhookRegistrationResponse>(
            HttpMethod.Put,
            $"/webhooks/{id}",
            config,
            cancellationToken);
    }
    
    /// <summary>
    /// List all registered webhooks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of webhook configurations</returns>
    public async Task<IList<WebhookRegistrationResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _client.RequestAsync<List<WebhookRegistrationResponse>>(
            HttpMethod.Get,
            "/webhooks",
            null,
            cancellationToken);
    }
    
    /// <summary>
    /// Delete a webhook
    /// </summary>
    /// <param name="id">Webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    public async Task<WebhookDeleteResponse> DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Webhook ID is required", nameof(id));
        }
        
        return await _client.RequestAsync<WebhookDeleteResponse>(
            HttpMethod.Delete,
            $"/webhooks/{id}",
            null,
            cancellationToken);
    }
    
    /// <summary>
    /// Verify a webhook signature
    /// </summary>
    /// <param name="signature">Signature from the X-CCAI-Signature header</param>
    /// <param name="body">Raw request body</param>
    /// <param name="secret">Webhook secret</param>
    /// <returns>Boolean indicating if the signature is valid</returns>
    public bool VerifySignature(string signature, string body, string secret)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(secret))
        {
            return false;
        }
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
        
        return signature == computedSignature;
    }
    
    /// <summary>
    /// Parse webhook event from JSON
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>Parsed webhook event</returns>
    public WebhookEventBase ParseEvent(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("JSON is required", nameof(json));
        }
        
        // First parse as a dynamic object to get the type
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        
        if (!jsonElement.TryGetProperty("type", out var typeElement))
        {
            throw new InvalidOperationException("Event type not found in webhook payload");
        }
        
        var typeString = typeElement.GetString();
        
        if (typeString == "message.sent")
        {
            return JsonSerializer.Deserialize<MessageSentEvent>(json)
                ?? throw new InvalidOperationException("Failed to deserialize MessageSentEvent");
        }
        else if (typeString == "message.received")
        {
            return JsonSerializer.Deserialize<MessageReceivedEvent>(json)
                ?? throw new InvalidOperationException("Failed to deserialize MessageReceivedEvent");
        }
        else
        {
            throw new InvalidOperationException($"Unknown event type: {typeString}");
        }
    }
    
    /// <summary>
    /// Register a new webhook endpoint (synchronous version)
    /// </summary>
    /// <param name="config">Webhook configuration</param>
    /// <returns>Registered webhook details</returns>
    public WebhookRegistrationResponse Register(WebhookConfig config)
    {
        return RegisterAsync(config).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Update an existing webhook configuration (synchronous version)
    /// </summary>
    /// <param name="id">Webhook ID</param>
    /// <param name="config">Updated webhook configuration</param>
    /// <returns>Updated webhook details</returns>
    public WebhookRegistrationResponse Update(string id, WebhookConfig config)
    {
        return UpdateAsync(id, config).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// List all registered webhooks (synchronous version)
    /// </summary>
    /// <returns>Array of webhook configurations</returns>
    public IList<WebhookRegistrationResponse> List()
    {
        return ListAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Delete a webhook (synchronous version)
    /// </summary>
    /// <param name="id">Webhook ID</param>
    /// <returns>Success message</returns>
    public WebhookDeleteResponse Delete(string id)
    {
        return DeleteAsync(id).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Response from webhook registration or update
/// </summary>
public record WebhookRegistrationResponse
{
    /// <summary>
    /// Webhook ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Webhook URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
    
    /// <summary>
    /// Subscribed events
    /// </summary>
    [JsonPropertyName("events")]
    public IList<WebhookEventType> Events { get; init; } = new List<WebhookEventType>();
}

/// <summary>
/// Response from webhook deletion
/// </summary>
public record WebhookDeleteResponse
{
    /// <summary>
    /// Success status
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }
    
    /// <summary>
    /// Response message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
