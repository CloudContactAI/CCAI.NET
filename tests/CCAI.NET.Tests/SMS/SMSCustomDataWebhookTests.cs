// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using CCAI.NET.SMS;
using Moq;
using Xunit;

namespace CCAI.NET.Tests.SMS;

public class SMSCustomDataWebhookTests
{
    private readonly Mock<CCAIClient> _mockClient;
    private readonly SMSService _smsService;
    
    public SMSCustomDataWebhookTests()
    {
        _mockClient = new Mock<CCAIClient>(
            new CCAIConfig { ClientId = "test-client-id", ApiKey = "test-api-key" },
            (HttpClient?)null
        );
        
        _mockClient.Setup(c => c.GetClientId()).Returns("test-client-id");
        
        _smsService = new SMSService(_mockClient.Object);
    }
    
    [Fact]
    public async Task SendAsync_WithCustomDataAndExternalClientId_IncludesCustomDataInRequest()
    {
        // Arrange
        var customAccountId = "TestAccount123";
        var customData = "WebhookTestData123";
        
        var account = new Account
        {
            FirstName = "John",
            LastName = "Test",
            Phone = "+15551234567",
            CustomAccountId = customAccountId,
            CustomData = customData
        };
        
        var request = SMSRequest.Create(
            accounts: new[] { account },
            message: "Hello ${FirstName}, this is a test with custom data!",
            title: "Custom Data Test Campaign",
            customData: customData
        );
        
        var expectedResponse = new SMSResponse
        {
            Id = "msg-123",
            Status = "sent",
            CampaignId = "camp-456",
            MessagesSent = 1,
            Timestamp = "2025-06-06T12:00:00Z"
        };
        
        object? capturedRequestBody = null;
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .Callback<HttpMethod, string, object, CancellationToken, Dictionary<string, string>>(
                (method, url, body, token, headers) => capturedRequestBody = body)
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _smsService.SendAsync(request);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
        Assert.Equal("camp-456", result.CampaignId);
        
        // Verify the request body contains our custom data
        Assert.NotNull(capturedRequestBody);
        var requestJson = JsonSerializer.Serialize(capturedRequestBody);
        Assert.Contains(customAccountId, requestJson);
        Assert.Contains(customData, requestJson);
        
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SendSingleAsync_WithCustomDataAndExternalClientId_CreatesCorrectRequest()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Test";
        var phone = "+15551234567";
        var message = "Hello ${FirstName}, webhook test with custom data!";
        var title = "Webhook Test Campaign";
        var customAccountId = "TestAccount123";
        var customData = "WebhookTestData123";
        
        var expectedResponse = new SMSResponse
        {
            Id = "msg-webhook-123",
            Status = "sent",
            CampaignId = "camp-webhook-456",
            MessagesSent = 1,
            Timestamp = DateTime.UtcNow.ToString("o")
        };
        
        object? capturedRequestBody = null;
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .Callback<HttpMethod, string, object, CancellationToken, Dictionary<string, string>>(
                (method, url, body, token, headers) => capturedRequestBody = body)
            .ReturnsAsync(expectedResponse);
        
        // Act
        var request = SMSRequest.CreateSingle(
            firstName: firstName,
            lastName: lastName,
            phone: phone,
            message: message,
            title: title,
            customAccountId: customAccountId,
            customData: customData
        );
        
        var result = await _smsService.SendAsync(request);
        
        // Assert
        Assert.Equal("msg-webhook-123", result.Id);
        Assert.Equal("sent", result.Status);
        Assert.Equal("camp-webhook-456", result.CampaignId);
        
        // Verify the request contains the custom data
        Assert.NotNull(capturedRequestBody);
        var requestJson = JsonSerializer.Serialize(capturedRequestBody);
        Assert.Contains("TestAccount123", requestJson);
        Assert.Contains("clientExternalId", requestJson);
        Assert.Contains("messageData", requestJson);
        
        // Verify the account in the request has the correct custom fields
        Assert.Single(request.Accounts);
        var account = request.Accounts.First();
        Assert.Equal(customAccountId, account.CustomAccountId);
        Assert.Equal(customData, account.CustomData);
        Assert.Equal(customData, request.CustomData);
    }
    
    [Fact]
    public void SMSRequest_CreateSingle_WithCustomData_SetsAllFields()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Test";
        var phone = "+15551234567";
        var message = "Test message with ${FirstName}";
        var title = "Test Campaign";
        var customAccountId = "TestAccount123";
        var customData = "WebhookTestData123";
        
        // Act
        var request = SMSRequest.CreateSingle(
            firstName: firstName,
            lastName: lastName,
            phone: phone,
            message: message,
            title: title,
            customAccountId: customAccountId,
            customData: customData
        );
        
        // Assert
        Assert.Equal(message, request.Message);
        Assert.Equal(title, request.Title);
        Assert.Equal(customData, request.CustomData);
        
        Assert.Single(request.Accounts);
        var account = request.Accounts.First();
        Assert.Equal(firstName, account.FirstName);
        Assert.Equal(lastName, account.LastName);
        Assert.Equal(phone, account.Phone);
        Assert.Equal(customAccountId, account.CustomAccountId);
        Assert.Equal(customData, account.CustomData);
    }
}