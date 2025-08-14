// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using CCAI.NET.SMS;
using Moq;
using Xunit;

namespace CCAI.NET.Tests.SMS;

public class SMSServiceTests
{
    private readonly Mock<CCAIClient> _mockClient;
    private readonly SMSService _smsService;
    
    public SMSServiceTests()
    {
        _mockClient = new Mock<CCAIClient>(
            new CCAIConfig { ClientId = "test-client-id", ApiKey = "test-api-key" },
            (HttpClient?)null
        );
        
        _mockClient.Setup(c => c.GetClientId()).Returns("test-client-id");
        
        _smsService = new SMSService(_mockClient.Object);
    }
    
    [Fact]
    public async Task SendAsync_WithValidInputs_CallsClientRequestAsync()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+15551234567"
            }
        };
        
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "Test Campaign";
        
        var expectedResponse = new SMSResponse
        {
            Id = "msg-123",
            Status = "sent",
            CampaignId = "camp-456",
            MessagesSent = 1,
            Timestamp = "2025-06-06T12:00:00Z"
        };
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _smsService.SendAsync(accounts, message, title);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
        Assert.Equal("camp-456", result.CampaignId);
        Assert.Equal(1, result.MessagesSent);
        Assert.Equal("2025-06-06T12:00:00Z", result.Timestamp);
        
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SendAsync_WithProgressTracking_NotifiesProgress()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+15551234567"
            }
        };
        
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "Test Campaign";
        
        var progressUpdates = new List<string>();
        var options = new SMSOptions
        {
            OnProgress = status => progressUpdates.Add(status)
        };
        
        var expectedResponse = new SMSResponse
        {
            Id = "msg-123",
            Status = "sent"
        };
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _smsService.SendAsync(accounts, message, title, options);
        
        // Assert
        Assert.Equal(3, progressUpdates.Count);
        Assert.Equal("Preparing to send SMS", progressUpdates[0]);
        Assert.Equal("Sending SMS", progressUpdates[1]);
        Assert.Equal("SMS sent successfully", progressUpdates[2]);
    }
    
    [Fact]
    public async Task SendSingleAsync_WithValidInputs_CallsSendAsync()
    {
        // Arrange
        var firstName = "Jane";
        var lastName = "Smith";
        var phone = "+15559876543";
        var message = "Hi ${FirstName}, thanks for your interest!";
        var title = "Single Message Test";
        
        var expectedResponse = new SMSResponse
        {
            Id = "msg-123",
            Status = "sent"
        };
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _smsService.SendSingleAsync(firstName, lastName, phone, message, title);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
        
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SendAsync_WithEmptyAccounts_ThrowsArgumentException()
    {
        // Arrange
        var accounts = new List<Account>();
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "Test Campaign";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _smsService.SendAsync(accounts, message, title));
        
        Assert.Equal("accounts", exception.ParamName);
    }
    
    [Fact]
    public async Task SendAsync_WithNullAccounts_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<Account>? accounts = null;
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "Test Campaign";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _smsService.SendAsync(accounts!, message, title));
        
        Assert.Equal("accounts", exception.ParamName);
    }
    
    [Fact]
    public async Task SendAsync_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+15551234567"
            }
        };
        
        var message = "";
        var title = "Test Campaign";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _smsService.SendAsync(accounts, message, title));
        
        Assert.Equal("message", exception.ParamName);
    }
    
    [Fact]
    public async Task SendAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+15551234567"
            }
        };
        
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _smsService.SendAsync(accounts, message, title));
        
        Assert.Equal("title", exception.ParamName);
    }
    
    [Fact]
    public async Task SendAsync_WithApiError_NotifiesProgressAndThrowsException()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Phone = "+15551234567"
            }
        };
        
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "Test Campaign";
        
        var progressUpdates = new List<string>();
        var options = new SMSOptions
        {
            OnProgress = status => progressUpdates.Add(status)
        };
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new HttpRequestException("API Error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _smsService.SendAsync(accounts, message, title, options));
        
        Assert.Contains("Failed to send SMS", exception.Message);
        Assert.Equal(3, progressUpdates.Count);
        Assert.Equal("Preparing to send SMS", progressUpdates[0]);
        Assert.Equal("Sending SMS", progressUpdates[1]);
        Assert.Equal("SMS sending failed", progressUpdates[2]);
    }
}
