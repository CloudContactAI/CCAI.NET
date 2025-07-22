// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;

namespace CCAI.NET.Tests;

/// <summary>
/// Tests for the CCAIClient class
/// </summary>
public class CCAIClientTests
{
    [Fact]
    public void Constructor_ValidConfig_InitializesServices()
    {
        // Arrange & Act
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        });
        
        // Assert
        Assert.NotNull(client.SMS);
        Assert.NotNull(client.MMS);
        Assert.NotNull(client.Email);
        Assert.NotNull(client.Webhook);
        Assert.Equal("test-client-id", client.GetClientId());
        Assert.Equal("test-api-key", client.GetApiKey());
        Assert.Equal("https://core.cloudcontactai.com/api", client.GetBaseUrl());
    }
    
    [Fact]
    public void Constructor_CustomBaseUrl_SetsBaseUrl()
    {
        // Arrange & Act
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key",
            BaseUrl = "https://custom-api.example.com"
        });
        
        // Assert
        Assert.Equal("https://custom-api.example.com", client.GetBaseUrl());
    }
    
    [Fact]
    public void Constructor_MissingClientId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CCAIClient(new CCAIConfig
        {
            ClientId = "",
            ApiKey = "test-api-key"
        }));
        
        Assert.Equal("Client ID is required (Parameter 'config.ClientId')", exception.Message);
    }
    
    [Fact]
    public void Constructor_MissingApiKey_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = ""
        }));
        
        Assert.Equal("API Key is required (Parameter 'config.ApiKey')", exception.Message);
    }
    
    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new CCAIClient(null!));
        
        Assert.Equal("Value cannot be null. (Parameter 'config')", exception.Message);
    }
    
    [Fact]
    public async Task RequestAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });
        
        var httpClient = new HttpClient(mockHandler.Object);
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        }, httpClient);
        
        // Act
        var result = await client.RequestAsync<Dictionary<string, JsonElement>>(
            HttpMethod.Get,
            "/test-endpoint"
        );
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("success"));
        
        mockHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == "https://core.cloudcontactai.com/api/test-endpoint"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task CustomRequestAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });
        
        var httpClient = new HttpClient(mockHandler.Object);
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        }, httpClient);
        
        var customBaseUrl = "https://email-campaigns.cloudcontactai.com/api/v1";
        
        // Act
        var result = await client.CustomRequestAsync<Dictionary<string, JsonElement>>(
            HttpMethod.Post,
            "/campaigns",
            new { test = true },
            customBaseUrl
        );
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("success"));
        
        mockHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == "https://email-campaigns.cloudcontactai.com/api/v1/campaigns"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task CustomRequestAsync_NoCustomBaseUrl_UsesDefaultBaseUrl()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\":true}")
            });
        
        var httpClient = new HttpClient(mockHandler.Object);
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        }, httpClient);
        
        // Act
        var result = await client.CustomRequestAsync<Dictionary<string, JsonElement>>(
            HttpMethod.Post,
            "/test-endpoint",
            new { test = true },
            null
        );
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("success"));
        
        mockHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == "https://core.cloudcontactai.com/api/test-endpoint"
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }
    
    [Fact]
    public async Task RequestAsync_HttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\":\"Bad Request\"}")
            });
        
        var httpClient = new HttpClient(mockHandler.Object);
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        }, httpClient);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.RequestAsync<Dictionary<string, JsonElement>>(
                HttpMethod.Get,
                "/test-endpoint"
            )
        );
    }
    
    [Fact]
    public void Dispose_DisposesHttpClient()
    {
        // Arrange
        var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        });
        
        // Act & Assert (no exception should be thrown)
        client.Dispose();
    }
}
