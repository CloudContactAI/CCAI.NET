// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Xunit;

namespace CCAI.NET.Tests;

public class CCAIClientTests
{
    [Fact]
    public void Constructor_WithValidConfig_CreatesClient()
    {
        // Arrange
        var config = new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        };
        
        // Act
        using var client = new CCAIClient(config);
        
        // Assert
        Assert.Equal("test-client-id", client.GetClientId());
        Assert.Equal("test-api-key", client.GetApiKey());
        Assert.Equal("https://core.cloudcontactai.com/api", client.GetBaseUrl());
    }
    
    [Fact]
    public void Constructor_WithCustomBaseUrl_UsesCustomUrl()
    {
        // Arrange
        var config = new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key",
            BaseUrl = "https://custom-api.example.com"
        };
        
        // Act
        using var client = new CCAIClient(config);
        
        // Assert
        Assert.Equal("https://custom-api.example.com", client.GetBaseUrl());
    }
    
    [Fact]
    public void Constructor_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new CCAIClient(null!));
        Assert.Equal("config", exception.ParamName);
    }
    
    [Fact]
    public void Constructor_WithEmptyClientId_ThrowsArgumentException()
    {
        // Arrange
        var config = new CCAIConfig
        {
            ClientId = "",
            ApiKey = "test-api-key"
        };
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CCAIClient(config));
        Assert.Equal("ClientId", exception.ParamName);
    }
    
    [Fact]
    public void Constructor_WithEmptyApiKey_ThrowsArgumentException()
    {
        // Arrange
        var config = new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = ""
        };
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new CCAIClient(config));
        Assert.Equal("ApiKey", exception.ParamName);
    }
    
    [Fact]
    public async Task RequestAsync_WithSuccessfulResponse_ReturnsDeserializedResponse()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"id\":\"test-id\",\"status\":\"success\"}")
        };
        
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        var httpClient = new HttpClient(handlerMock.Object);
        var config = new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        };
        
        using var client = new CCAIClient(config, httpClient);
        
        // Act
        var result = await client.RequestAsync<Dictionary<string, JsonElement>>(
            HttpMethod.Get,
            "/test-endpoint"
        );
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("id"));
        Assert.True(result.ContainsKey("status"));
        Assert.Equal("test-id", result["id"].GetString());
        Assert.Equal("success", result["status"].GetString());
        
        // Verify the request was made correctly
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString() == "https://core.cloudcontactai.com/api/test-endpoint" &&
                req.Headers.Authorization!.Scheme == "Bearer" &&
                req.Headers.Authorization!.Parameter == "test-api-key"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    [Fact]
    public async Task RequestAsync_WithErrorResponse_ThrowsHttpRequestException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent("{\"error\":\"Bad request\"}")
        };
        
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        var httpClient = new HttpClient(handlerMock.Object);
        var config = new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        };
        
        using var client = new CCAIClient(config, httpClient);
        
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.RequestAsync<Dictionary<string, JsonElement>>(
            HttpMethod.Get,
            "/test-endpoint"
        ));
    }
}
