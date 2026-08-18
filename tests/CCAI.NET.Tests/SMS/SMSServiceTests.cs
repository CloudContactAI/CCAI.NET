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
    private readonly Mock<ICCAIClient> _mockClient;
    private readonly ISMSService _smsService;

    public SMSServiceTests()
    {
        _mockClient = new Mock<ICCAIClient>();

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
        var request = SMSRequest.Create(accounts, message, title);
        var result = await _smsService.SendAsync(request);
        
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
        var request = SMSRequest.Create(accounts, message, title, null, options);
        var result = await _smsService.SendAsync(request);
        
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
        var request = SMSRequest.CreateSingle(firstName, lastName, phone, message, title);
        var result = await _smsService.SendAsync(request);
        
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
        var request = SMSRequest.Create(accounts, message, title);
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _smsService.SendAsync(request));
        
        Assert.Contains("Account", exception.ParamName);
    }
    
    [Fact]
    public async Task SendAsync_WithNullAccounts_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<Account>? accounts = null;
        var message = "Hello ${FirstName}, this is a test message!";
        var title = "Test Campaign";
        
        // Act & Assert
        var request = SMSRequest.Create(accounts!, message, title);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _smsService.SendAsync(request));
        
        Assert.Contains("Account", exception.ParamName);
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
        var request = SMSRequest.Create(accounts, message, title);
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _smsService.SendAsync(request));
        
        Assert.Contains("Message", exception.ParamName);
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
        var request = SMSRequest.Create(accounts, message, title);
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _smsService.SendAsync(request));
        
        Assert.Contains("Title", exception.ParamName);
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
        var request = SMSRequest.Create(accounts, message, title, null, options);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _smsService.SendAsync(request));
        
        Assert.Contains("Failed to send SMS", exception.Message);
        Assert.Equal(3, progressUpdates.Count);
        Assert.Equal("Preparing to send SMS", progressUpdates[0]);
        Assert.Equal("Sending SMS", progressUpdates[1]);
        Assert.Equal("SMS sending failed", progressUpdates[2]);
    }
    
    [Fact]
    public async Task SendAsync_WithSMSRequest_CallsClientRequestAsync()
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
        
        var request = SMSRequest.Create(accounts, "Hello ${FirstName}!", "Test Campaign");
        
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
        var result = await _smsService.SendAsync(request);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
    }
    
    [Fact]
    public async Task SendAsync_WithSMSRequestProgressTracking_NotifiesProgress()
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

        var progressUpdates = new List<string>();
        var options = new SMSOptions
        {
            OnProgress = status => progressUpdates.Add(status)
        };

        var request = SMSRequest.Create(accounts, "Hello ${FirstName}!", "Test Campaign", null, options);

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
        var result = await _smsService.SendAsync(request);

        // Assert
        Assert.Equal(3, progressUpdates.Count);
        Assert.Equal("Preparing to send SMS", progressUpdates[0]);
        Assert.Equal("Sending SMS", progressUpdates[1]);
        Assert.Equal("SMS sent successfully", progressUpdates[2]);
    }

    [Fact]
    public async Task SendAsync_WithData_IncludesDataInPayload()
    {
        // Arrange
        object? capturedData = null;
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            Phone = "+15551234567",
            Data = new Dictionary<string, string>
            {
                { "city", "Miami" },
                { "country", "USA" },
                { "plan", "premium" }
            }
        };

        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .Callback<HttpMethod, string, object, CancellationToken, Dictionary<string, string>>(
                (_, _, data, _, _) => capturedData = data)
            .ReturnsAsync(new SMSResponse { Id = "msg-1", Status = "sent" });

        // Act
        var request = SMSRequest.Create(new[] { account }, "Hello ${firstName} from ${city}!", "Test");
        await _smsService.SendAsync(request);

        // Assert
        Assert.NotNull(capturedData);
        var campaign = capturedData as SMSCampaign;
        Assert.NotNull(campaign);
        var sentAccount = campaign!.Accounts.First();
        Assert.NotNull(sentAccount.Data);
        Assert.Equal("Miami", sentAccount.Data["city"]);
        Assert.Equal("USA", sentAccount.Data["country"]);
        Assert.Equal("premium", sentAccount.Data["plan"]);

        // Verify JSON uses "data" key (API wire format)
        var json = JsonSerializer.Serialize(sentAccount);
        Assert.Contains("\"data\"", json);
        Assert.Contains("\"city\":\"Miami\"", json);
    }

    [Fact]
    public async Task SendAsync_ReturnsMessageAndResponseId()
    {
        // Arrange
        var expectedResponse = new SMSResponse
        {
            Id = "msg-123",
            Status = "sent",
            Message = "SMS sent successfully",
            ResponseId = "resp-abc-123"
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
        var account = new Account { FirstName = "John", LastName = "Doe", Phone = "+15551234567" };
        var request = SMSRequest.Create(new[] { account }, "Hello!", "Test");
        var result = await _smsService.SendAsync(request);

        // Assert
        Assert.Equal("SMS sent successfully", result.Message);
        Assert.Equal("resp-abc-123", result.ResponseId);
    }

    [Fact]
    public async Task SendWithTemplateAsync_ShouldIncludeTemplateId()
    {
        // Arrange
        var expectedResponse = new SMSResponse { Id = "msg-tpl-1", Status = "sent", CampaignId = "camp-tpl-1" };
        _mockClient.Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);

        var accounts = new[] { new Account { FirstName = "John", LastName = "Doe", Phone = "+15551234567" } };

        // Act
        var result = await _smsService.SendWithTemplateAsync(accounts, 12345L, "Template Campaign");

        // Assert
        Assert.Equal("msg-tpl-1", result.Id);
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.Is<object>(body => body.ToString()!.Contains("12345")),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task SendSingleWithTemplateAsync_ShouldSendToSingleRecipient()
    {
        // Arrange
        var expectedResponse = new SMSResponse { Id = "msg-tpl-2", Status = "sent" };
        _mockClient.Setup(c => c.RequestAsync<SMSResponse>(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _smsService.SendSingleWithTemplateAsync("Jane", "Smith", "+15559876543", 99L, "Single Template");

        // Assert
        Assert.Equal("msg-tpl-2", result.Id);
    }
}
