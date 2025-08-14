// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
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
    private readonly WebhookService _webhookService;
    
    public WebhookServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        _client = new CCAIClient(
            new CCAIConfig
            {
                ClientId = "test-client-id",
                ApiKey = "test-api-key"
            },
            _httpClient
        );
        
        _webhookService = _client.Webhook;
    }
    
    [Fact]
    public async Task RegisterAsync_ValidConfig_ReturnsResponse()
    {
        // Arrange
        var config = new WebhookConfig
        {
            Url = "https://example.com/webhook",
            Events = new List<WebhookEventType> { WebhookEventType.MessageSent },
            Secret = "test-secret"
        };
        
        var responseContent = new WebhookRegistrationResponse
        {
            Id = "webhook-123",
            Url = "https://example.com/webhook",
            Events = new List<WebhookEventType> { WebhookEventType.MessageSent }
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });
        
        // Act
        var result = await _webhookService.RegisterAsync(config);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("webhook-123", result.Id);
        Assert.Equal("https://example.com/webhook", result.Url);
        Assert.Single(result.Events);
        Assert.Equal(WebhookEventType.MessageSent, result.Events[0]);
        
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == "https://core.cloudcontactai.com/api/webhooks"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task RegisterAsync_MissingUrl_ThrowsArgumentException()
    {
        // Arrange
        var config = new WebhookConfig
        {
            Url = "",
            Events = new List<WebhookEventType> { WebhookEventType.MessageSent }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.RegisterAsync(config));
        
        Assert.Equal("URL is required (Parameter 'config.Url')", exception.Message);
    }
    
    [Fact]
    public async Task RegisterAsync_NoEvents_ThrowsArgumentException()
    {
        // Arrange
        var config = new WebhookConfig
        {
            Url = "https://example.com/webhook",
            Events = new List<WebhookEventType>()
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.RegisterAsync(config));
        
        Assert.Equal("At least one event type is required (Parameter 'config.Events')", exception.Message);
    }
    
    [Fact]
    public async Task UpdateAsync_ValidIdAndConfig_ReturnsResponse()
    {
        // Arrange
        var webhookId = "webhook-123";
        var config = new WebhookConfig
        {
            Url = "https://example.com/webhook-updated",
            Events = new List<WebhookEventType> { WebhookEventType.MessageReceived },
            Secret = "test-secret-updated"
        };
        
        var responseContent = new WebhookRegistrationResponse
        {
            Id = webhookId,
            Url = "https://example.com/webhook-updated",
            Events = new List<WebhookEventType> { WebhookEventType.MessageReceived }
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });
        
        // Act
        var result = await _webhookService.UpdateAsync(webhookId, config);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(webhookId, result.Id);
        Assert.Equal("https://example.com/webhook-updated", result.Url);
        Assert.Single(result.Events);
        Assert.Equal(WebhookEventType.MessageReceived, result.Events[0]);
        
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString() == $"https://core.cloudcontactai.com/api/webhooks/{webhookId}"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task UpdateAsync_MissingId_ThrowsArgumentException()
    {
        // Arrange
        var config = new WebhookConfig
        {
            Url = "https://example.com/webhook",
            Events = new List<WebhookEventType> { WebhookEventType.MessageSent }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.UpdateAsync("", config));
        
        Assert.Equal("Webhook ID is required (Parameter 'id')", exception.Message);
    }
    
    [Fact]
    public async Task ListAsync_ReturnsWebhooks()
    {
        // Arrange
        var responseContent = new List<WebhookRegistrationResponse>
        {
            new WebhookRegistrationResponse
            {
                Id = "webhook-123",
                Url = "https://example.com/webhook1",
                Events = new List<WebhookEventType> { WebhookEventType.MessageSent }
            },
            new WebhookRegistrationResponse
            {
                Id = "webhook-456",
                Url = "https://example.com/webhook2",
                Events = new List<WebhookEventType> { WebhookEventType.MessageReceived }
            }
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });
        
        // Act
        var result = await _webhookService.ListAsync();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("webhook-123", result[0].Id);
        Assert.Equal("webhook-456", result[1].Id);
        
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == "https://core.cloudcontactai.com/api/webhooks"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsSuccessResponse()
    {
        // Arrange
        var webhookId = "webhook-123";
        var responseContent = new WebhookDeleteResponse
        {
            Success = true,
            Message = "Webhook deleted successfully"
        };
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });
        
        // Act
        var result = await _webhookService.DeleteAsync(webhookId);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Webhook deleted successfully", result.Message);
        
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri!.ToString() == $"https://core.cloudcontactai.com/api/webhooks/{webhookId}"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task DeleteAsync_MissingId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _webhookService.DeleteAsync(""));
        
        Assert.Equal("Webhook ID is required (Parameter 'id')", exception.Message);
    }
    
    [Fact]
    public void VerifySignature_ValidSignature_ReturnsTrue()
    {
        // Arrange
        var body = "{\"type\":\"message.sent\",\"message\":\"Hello\"}";
        var secret = "test-secret";
        
        // Compute a valid signature
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(body));
        var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
        
        // Act
        var result = _webhookService.VerifySignature(signature, body, secret);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void VerifySignature_InvalidSignature_ReturnsFalse()
    {
        // Arrange
        var body = "{\"type\":\"message.sent\",\"message\":\"Hello\"}";
        var secret = "test-secret";
        var invalidSignature = "invalid-signature";
        
        // Act
        var result = _webhookService.VerifySignature(invalidSignature, body, secret);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ParseEvent_MessageSentEvent_ReturnsCorrectType()
    {
        // Arrange
        var json = @"{
            ""type"": ""message.sent"",
            ""campaign"": {
                ""id"": 12345,
                ""title"": ""Test Campaign"",
                ""message"": ""Hello ${FirstName}, this is a test message."",
                ""senderPhone"": ""+15551234567"",
                ""createdAt"": ""2025-07-22T12:00:00Z"",
                ""runAt"": ""2025-07-22T12:01:00Z""
            },
            ""from"": ""+15551234567"",
            ""to"": ""+15559876543"",
            ""message"": ""Hello John, this is a test message.""
        }";
        
        // Act
        var result = _webhookService.ParseEvent(json);
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<MessageSentEvent>(result);
        Assert.Equal(WebhookEventType.MessageSent, result.Type);
        Assert.Equal(12345, result.Campaign.Id);
        Assert.Equal("Test Campaign", result.Campaign.Title);
        Assert.Equal("+15551234567", result.From);
        Assert.Equal("+15559876543", result.To);
        Assert.Equal("Hello John, this is a test message.", result.Message);
    }
    
    [Fact]
    public void ParseEvent_MessageReceivedEvent_ReturnsCorrectType()
    {
        // Arrange
        var json = @"{
            ""type"": ""message.received"",
            ""campaign"": {
                ""id"": 12345,
                ""title"": ""Test Campaign"",
                ""message"": ""Hello ${FirstName}, this is a test message."",
                ""senderPhone"": ""+15551234567"",
                ""createdAt"": ""2025-07-22T12:00:00Z"",
                ""runAt"": ""2025-07-22T12:01:00Z""
            },
            ""from"": ""+15559876543"",
            ""to"": ""+15551234567"",
            ""message"": ""Yes, I received your message.""
        }";
        
        // Act
        var result = _webhookService.ParseEvent(json);
        
        // Assert
        Assert.NotNull(result);
        Assert.IsType<MessageReceivedEvent>(result);
        Assert.Equal(WebhookEventType.MessageReceived, result.Type);
        Assert.Equal(12345, result.Campaign.Id);
        Assert.Equal("Test Campaign", result.Campaign.Title);
        Assert.Equal("+15559876543", result.From);
        Assert.Equal("+15551234567", result.To);
        Assert.Equal("Yes, I received your message.", result.Message);
    }
    
    [Fact]
    public void ParseEvent_UnknownEventType_ThrowsException()
    {
        // Arrange
        var json = @"{
            ""type"": ""unknown.event"",
            ""campaign"": {
                ""id"": 12345,
                ""title"": ""Test Campaign""
            },
            ""message"": ""Test message""
        }";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _webhookService.ParseEvent(json));
        
        Assert.Equal("Unknown event type: unknown.event", exception.Message);
    }
    
    [Fact]
    public void ParseEvent_MissingType_ThrowsException()
    {
        // Arrange
        var json = @"{
            ""campaign"": {
                ""id"": 12345,
                ""title"": ""Test Campaign""
            },
            ""message"": ""Test message""
        }";
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _webhookService.ParseEvent(json));
        
        Assert.Equal("Event type not found in webhook payload", exception.Message);
    }
}
