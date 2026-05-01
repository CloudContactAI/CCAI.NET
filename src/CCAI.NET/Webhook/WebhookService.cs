// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.Webhook;

/// <summary>
/// Interface for service managing CloudContactAI webhooks
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Register a new webhook endpoint
    /// </summary>
    Task<WebhookRegistrationResponse> RegisterAsync(
        WebhookConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing webhook configuration
    /// </summary>
    Task<WebhookRegistrationResponse> UpdateAsync(
        int id,
        WebhookConfig config,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List all registered webhooks
    /// </summary>
    Task<IList<WebhookRegistrationResponse>> ListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a webhook
    /// </summary>
    Task<WebhookDeleteResponse> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify a webhook signature using HMAC-SHA256 with constant-time comparison
    /// </summary>
    bool VerifySignature(string signature, string clientId, string eventHash, string secret);

    /// <summary>
    /// Parse CloudContact webhook event from JSON
    /// </summary>
    CloudContactWebhookEvent ParseCloudContactEvent(string json);

    /// <summary>
    /// Parse webhook event from JSON (legacy format support)
    /// </summary>
    WebhookEventBase ParseEvent(string json);

    /// <summary>
    /// Register a new webhook endpoint (synchronous version)
    /// </summary>
    WebhookRegistrationResponse Register(WebhookConfig config);

    /// <summary>
    /// Update an existing webhook configuration (synchronous version)
    /// </summary>
    WebhookRegistrationResponse Update(int id, WebhookConfig config);

    /// <summary>
    /// List all registered webhooks (synchronous version)
    /// </summary>
    IList<WebhookRegistrationResponse> List();

    /// <summary>
    /// Delete a webhook (synchronous version)
    /// </summary>
    WebhookDeleteResponse Delete(int id);
}

/// <summary>
/// Service for managing CloudContactAI webhooks
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly ICCAIClient _client;

    /// <summary>
    /// Create a new Webhook service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public WebhookService(ICCAIClient client)
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

        var payload = new[]
        {
            new
            {
                url             = config.Url,
                method          = "POST",
                integrationType = config.IntegrationType ?? "ALL",
                secretKey       = config.Secret
            }
        };

        var endpoint = $"/v1/client/{_client.GetClientId()}/integration";
        var responses = await _client.RequestAsync<List<WebhookRegistrationResponse>>(
            HttpMethod.Post, endpoint, payload, cancellationToken);

        return responses.FirstOrDefault()
            ?? throw new InvalidOperationException("Empty response from register webhook");
    }

    /// <summary>
    /// Update an existing webhook configuration
    /// </summary>
    /// <param name="id">Webhook ID</param>
    /// <param name="config">Updated webhook configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated webhook details</returns>
    public async Task<WebhookRegistrationResponse> UpdateAsync(
        int id,
        WebhookConfig config,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Webhook ID is required", nameof(id));
        }

        var payload = new[]
        {
            new
            {
                id              = id,
                url             = config.Url,
                method          = "POST",
                integrationType = config.IntegrationType ?? "ALL",
                secretKey       = config.Secret
            }
        };

        var endpoint = $"/v1/client/{_client.GetClientId()}/integration";
        var responses = await _client.RequestAsync<List<WebhookRegistrationResponse>>(
            HttpMethod.Post, endpoint, payload, cancellationToken);

        return responses.FirstOrDefault()
            ?? throw new InvalidOperationException("Empty response from update webhook");
    }

    /// <summary>
    /// List all registered webhooks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of webhook configurations</returns>
    public async Task<IList<WebhookRegistrationResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"/v1/client/{_client.GetClientId()}/integration";
        return await _client.RequestAsync<List<WebhookRegistrationResponse>>(
            HttpMethod.Get, endpoint, null, cancellationToken);
    }

    /// <summary>
    /// Delete a webhook
    /// </summary>
    /// <param name="id">Webhook ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    public async Task<WebhookDeleteResponse> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Webhook ID is required", nameof(id));
        }

        var endpoint = $"/v1/client/{_client.GetClientId()}/integration/{id}";
        return await _client.RequestAsync<WebhookDeleteResponse>(
            HttpMethod.Delete, endpoint, null, cancellationToken);
    }

    /// <summary>
    /// Verify a webhook signature using HMAC-SHA256 with constant-time comparison
    /// </summary>
    /// <param name="signature">Signature from webhook (Base64 encoded)</param>
    /// <param name="clientId">Client ID from webhook</param>
    /// <param name="eventHash">Event hash from webhook</param>
    /// <param name="secret">Webhook secret</param>
    /// <returns>Boolean indicating if the signature is valid</returns>
    public bool VerifySignature(string signature, string clientId, string eventHash, string secret)
    {
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(clientId) ||
            string.IsNullOrEmpty(eventHash) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        try
        {
            var expectedSignature = GenerateSignature(secret, clientId, eventHash);
            return ConstantTimeEquals(signature, expectedSignature);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate the expected signature for a webhook event
    /// </summary>
    /// <param name="secret">Webhook secret</param>
    /// <param name="clientId">Client ID</param>
    /// <param name="eventHash">Event hash</param>
    /// <returns>Base64-encoded signature</returns>
    private string GenerateSignature(string secret, string clientId, string eventHash)
    {
        var data = $"{clientId}:{eventHash}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(signatureBytes);
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing attacks
    /// </summary>
    private bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }
    
    /// <summary>
    /// Parse CloudContact webhook event from JSON
    /// </summary>
    /// <param name="json">JSON string</param>
    /// <returns>Parsed CloudContact webhook event</returns>
    public CloudContactWebhookEvent ParseCloudContactEvent(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("JSON is required", nameof(json));
        }
        
        return JsonSerializer.Deserialize<CloudContactWebhookEvent>(json)
            ?? throw new InvalidOperationException("Failed to deserialize CloudContact webhook event");
    }
    
    /// <summary>
    /// Parse webhook event from JSON (legacy format support)
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
        
        // Check if this is the new CloudContact format
        if (jsonElement.TryGetProperty("eventType", out var eventTypeElement))
        {
            // This is the new format, convert to legacy format for backward compatibility
            var cloudContactEvent = ParseCloudContactEvent(json);
            return ConvertToLegacyEvent(cloudContactEvent);
        }
        
        // Legacy format
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
        else if (typeString == "message.received" || typeString == "message.incoming")
        {
            return JsonSerializer.Deserialize<MessageIncomingEvent>(json)
                ?? throw new InvalidOperationException("Failed to deserialize MessageIncomingEvent");
        }
        else if (typeString == "contact.unsubscribed")
        {
            return JsonSerializer.Deserialize<ContactUnsubscribedEvent>(json)
                ?? throw new InvalidOperationException("Failed to deserialize ContactUnsubscribedEvent");
        }
        else
        {
            throw new InvalidOperationException($"Unknown event type: {typeString}");
        }
    }
    
    /// <summary>
    /// Convert CloudContact webhook event to legacy format for backward compatibility
    /// </summary>
    /// <param name="cloudContactEvent">CloudContact webhook event</param>
    /// <returns>Legacy webhook event</returns>
    private WebhookEventBase ConvertToLegacyEvent(CloudContactWebhookEvent cloudContactEvent)
    {
        var campaign = new WebhookCampaign
        {
            Id = cloudContactEvent.Data.CampaignId,
            Title = cloudContactEvent.Data.CampaignTitle,
            Message = cloudContactEvent.Data.Message
        };
        
        return cloudContactEvent.EventType switch
        {
            "message.sent" => new MessageSentEvent
            {
                Campaign = campaign,
                From = cloudContactEvent.Data.From ?? string.Empty,
                To = cloudContactEvent.Data.To,
                Message = cloudContactEvent.Data.Message
            },
            "message.incoming" => new MessageIncomingEvent
            {
                Campaign = campaign,
                From = cloudContactEvent.Data.From ?? string.Empty,
                To = cloudContactEvent.Data.To,
                Message = cloudContactEvent.Data.Message
            },
            "contact.unsubscribed" => new ContactUnsubscribedEvent
            {
                Campaign = campaign,
                From = cloudContactEvent.Data.From ?? cloudContactEvent.Data.To,
                To = cloudContactEvent.Data.To,
                Message = cloudContactEvent.Data.Message,
                UnsubscribedAt = cloudContactEvent.Data.UnsubscribedAt ?? string.Empty,
                ContactData = cloudContactEvent.Data.ContactData ?? new ContactData()
            },
            _ => throw new InvalidOperationException($"Unsupported event type for legacy conversion: {cloudContactEvent.EventType}")
        };
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
    public WebhookRegistrationResponse Update(int id, WebhookConfig config)
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
    public WebhookDeleteResponse Delete(int id)
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
    public int Id { get; init; }

    /// <summary>
    /// Webhook URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// HTTP method (always POST)
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; init; } = "POST";

    /// <summary>
    /// Integration type filter (e.g. "DEFAULT", "SMS", "EMAIL")
    /// </summary>
    [JsonPropertyName("integrationType")]
    public string IntegrationType { get; init; } = "DEFAULT";

    /// <summary>
    /// Secret key used for signature verification
    /// </summary>
    [JsonPropertyName("secretKey")]
    public string? SecretKey { get; init; }
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
