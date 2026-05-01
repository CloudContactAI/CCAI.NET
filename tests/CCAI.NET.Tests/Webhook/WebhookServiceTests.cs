// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using CCAI.NET;
using CCAI.NET.Webhook;
using Moq;
using Moq.Protected;
using Xunit;

namespace CCAI.NET.Tests.Webhook;

/// <summary>
/// Tests for the WebhookService class
/// </summary>
public class WebhookServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CCAIClient _client;
    private readonly IWebhookService _webhookService;

    public WebhookServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _client = new CCAIClient(
            new CCAIConfig { ClientId = "test-client-id", ApiKey = "test-api-key" },
            _httpClient
        );

        _webhookService = _client.Webhook;
    }

    // ─── RegisterAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidConfig_PostsToCorrectEndpoint()
    {
        var config = new WebhookConfig
        {
            Url    = "https://example.com/webhook",
            Secret = "test-secret"
        };

        var responseContent = new List<WebhookRegistrationResponse>
        {
            new() { Id = 42, Url = "https://example.com/webhook", Method = "POST", IntegrationType = "ALL" }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(JsonSerializer.Serialize(responseContent))
            });

        var result = await _webhookService.RegisterAsync(config);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("https://example.com/webhook", result.Url);
        Assert.Equal("ALL", result.IntegrationType);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() ==
                        "https://core.cloudcontactai.com/api/v1/client/test-client-id/integration"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_MissingUrl_ThrowsArgumentException()
    {
        var config = new WebhookConfig { Url = "" };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.RegisterAsync(config));

        Assert.Equal("URL is required (Parameter 'Url')", exception.Message);
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidIdAndConfig_PostsToCorrectEndpoint()
    {
        var config = new WebhookConfig
        {
            Url    = "https://example.com/webhook-updated",
            Secret = "updated-secret"
        };

        var responseContent = new List<WebhookRegistrationResponse>
        {
            new() { Id = 42, Url = "https://example.com/webhook-updated", Method = "POST", IntegrationType = "ALL" }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(JsonSerializer.Serialize(responseContent))
            });

        var result = await _webhookService.UpdateAsync(42, config);

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("https://example.com/webhook-updated", result.Url);

        // Update also uses POST to the same integration endpoint (not PUT)
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() ==
                        "https://core.cloudcontactai.com/api/v1/client/test-client-id/integration"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_MissingId_ThrowsArgumentException()
    {
        var config = new WebhookConfig { Url = "https://example.com/webhook" };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.UpdateAsync(0, config));

        Assert.Equal("Webhook ID is required (Parameter 'id')", exception.Message);
    }

    // ─── ListAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_ReturnsWebhooks_FromCorrectEndpoint()
    {
        var responseContent = new List<WebhookRegistrationResponse>
        {
            new() { Id = 1, Url = "https://example.com/webhook1", IntegrationType = "ALL" },
            new() { Id = 2, Url = "https://example.com/webhook2", IntegrationType = "SMS" }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(JsonSerializer.Serialize(responseContent))
            });

        var result = await _webhookService.ListAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() ==
                        "https://core.cloudcontactai.com/api/v1/client/test-client-id/integration"),
                ItExpr.IsAny<CancellationToken>());
    }

    // ─── DeleteAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ValidId_DeletesFromCorrectEndpoint()
    {
        var responseContent = new WebhookDeleteResponse { Success = true, Message = "Deleted" };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content    = new StringContent(JsonSerializer.Serialize(responseContent))
            });

        var result = await _webhookService.DeleteAsync(42);

        Assert.True(result.Success);
        Assert.Equal("Deleted", result.Message);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString() ==
                        "https://core.cloudcontactai.com/api/v1/client/test-client-id/integration/42"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_MissingId_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.DeleteAsync(0));

        Assert.Equal("Webhook ID is required (Parameter 'id')", exception.Message);
    }

    // ─── VerifySignature ──────────────────────────────────────────────────

    [Fact]
    public void VerifySignature_ValidSignature_ReturnsTrue()
    {
        var clientId = "test-client-id";
        var eventHash = "event-hash-abc123";
        var secret = "test-secret-key";

        // Compute: HMAC-SHA256(secret, clientId:eventHash) in Base64
        var data = $"{clientId}:{eventHash}";
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            System.Text.Encoding.UTF8.GetBytes(secret));
        var signatureBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        var signature = Convert.ToBase64String(signatureBytes);

        Assert.True(_webhookService.VerifySignature(signature, clientId, eventHash, secret));
    }

    [Fact]
    public void VerifySignature_InvalidSignature_ReturnsFalse()
    {
        Assert.False(_webhookService.VerifySignature("invalid-signature", "client-123", "event-hash", "secret"));
    }

    [Fact]
    public void VerifySignature_EmptySignature_ReturnsFalse()
    {
        Assert.False(_webhookService.VerifySignature("", "client-123", "event-hash", "secret"));
    }

    [Fact]
    public void VerifySignature_EmptyClientId_ReturnsFalse()
    {
        Assert.False(_webhookService.VerifySignature("sig", "", "event-hash", "secret"));
    }

    [Fact]
    public void VerifySignature_EmptyEventHash_ReturnsFalse()
    {
        Assert.False(_webhookService.VerifySignature("sig", "client-123", "", "secret"));
    }

    [Fact]
    public void VerifySignature_EmptySecret_ReturnsFalse()
    {
        Assert.False(_webhookService.VerifySignature("sig", "client-123", "event-hash", ""));
    }

    // ─── ParseEvent ───────────────────────────────────────────────────────

    [Fact]
    public void ParseEvent_MessageSentEvent_ReturnsCorrectType()
    {
        var json = @"{
            ""type"": ""message.sent"",
            ""campaign"": {
                ""id"": 12345,
                ""title"": ""Test Campaign"",
                ""message"": ""Hello ${FirstName}!"",
                ""senderPhone"": ""+15551234567"",
                ""createdAt"": ""2025-07-22T12:00:00Z"",
                ""runAt"": ""2025-07-22T12:01:00Z""
            },
            ""from"": ""+15551234567"",
            ""to"": ""+15559876543"",
            ""message"": ""Hello John!""
        }";

        var result = _webhookService.ParseEvent(json);

        Assert.NotNull(result);
        Assert.IsType<MessageSentEvent>(result);
        Assert.Equal(WebhookEventType.MessageSent, result.Type);
        Assert.Equal(12345, result.Campaign.Id);
        Assert.Equal("Test Campaign", result.Campaign.Title);
        Assert.Equal("+15551234567", result.From);
        Assert.Equal("+15559876543", result.To);
        Assert.Equal("Hello John!", result.Message);
    }

    [Fact]
    public void ParseEvent_MessageIncomingEvent_ReturnsCorrectType()
    {
        var json = @"{
            ""type"": ""message.received"",
            ""campaign"": { ""id"": 1, ""title"": ""Test"" },
            ""from"": ""+15559876543"",
            ""to"":   ""+15551234567"",
            ""message"": ""Reply here""
        }";

        var result = _webhookService.ParseEvent(json);

        Assert.NotNull(result);
        Assert.IsType<MessageIncomingEvent>(result);
        Assert.Equal(WebhookEventType.MessageIncoming, result.Type);
    }

    [Fact]
    public void ParseEvent_UnknownEventType_ThrowsException()
    {
        var json = @"{ ""type"": ""unknown.event"", ""campaign"": { ""id"": 1 }, ""message"": ""test"" }";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _webhookService.ParseEvent(json));

        Assert.Equal("Unknown event type: unknown.event", exception.Message);
    }

    [Fact]
    public void ParseEvent_MissingType_ThrowsException()
    {
        var json = @"{ ""campaign"": { ""id"": 1 }, ""message"": ""test"" }";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _webhookService.ParseEvent(json));

        Assert.Equal("Event type not found in webhook payload", exception.Message);
    }
}
