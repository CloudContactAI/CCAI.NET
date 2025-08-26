// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using CCAI.NET.Email;
using Moq;
using Moq.Protected;
using Xunit;

namespace CCAI.NET.Tests.Email;

/// <summary>
/// Tests for the EmailService class
/// </summary>
public class EmailServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CCAIClient _client;
    private readonly EmailService _emailService;
    
    public EmailServiceTests()
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
        
        _emailService = _client.Email;
    }
    
    [Fact]
    public async Task SendCampaignAsync_ValidCampaign_ReturnsResponse()
    {
        // Arrange
        var campaign = new EmailCampaign
        {
            Subject = "Test Subject",
            Title = "Test Campaign",
            Message = "<p>Test message</p>",
            SenderEmail = "sender@example.com",
            ReplyEmail = "reply@example.com",
            SenderName = "Test Sender",
            Accounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com"
                }
            }
        };
        
        var responseContent = new EmailResponse
        {
            Id = "campaign-123",
            Status = "success",
            MessagesSent = 1
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
        var result = await _emailService.SendCampaignAsync(campaign);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("campaign-123", result.Id);
        Assert.Equal("success", result.Status);
        Assert.Equal(1, result.MessagesSent);
        
        _mockHttpMessageHandler
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
    public async Task SendCampaignAsync_WithOptions_CallsProgressCallback()
    {
        // Arrange
        var campaign = new EmailCampaign
        {
            Subject = "Test Subject",
            Title = "Test Campaign",
            Message = "<p>Test message</p>",
            SenderEmail = "sender@example.com",
            ReplyEmail = "reply@example.com",
            SenderName = "Test Sender",
            Accounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com"
                }
            }
        };
        
        var responseContent = new EmailResponse
        {
            Id = "campaign-123",
            Status = "success",
            MessagesSent = 1
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
        
        var progressMessages = new List<string>();
        var options = new EmailOptions
        {
            OnProgress = message => progressMessages.Add(message)
        };
        
        // Act
        var result = await _emailService.SendCampaignAsync(campaign, options);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, progressMessages.Count);
        Assert.Equal("Preparing to send email campaign", progressMessages[0]);
        Assert.Equal("Sending email campaign", progressMessages[1]);
        Assert.Equal("Email campaign sent successfully", progressMessages[2]);
    }
    
    [Fact]
    public async Task SendCampaignAsync_ApiError_ThrowsException()
    {
        // Arrange
        var campaign = new EmailCampaign
        {
            Subject = "Test Subject",
            Title = "Test Campaign",
            Message = "<p>Test message</p>",
            SenderEmail = "sender@example.com",
            ReplyEmail = "reply@example.com",
            SenderName = "Test Sender",
            Accounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com"
                }
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
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\":\"Invalid email format\"}")
            });
        
        var progressMessages = new List<string>();
        var options = new EmailOptions
        {
            OnProgress = message => progressMessages.Add(message)
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _emailService.SendCampaignAsync(campaign, options));
        
        Assert.Equal(3, progressMessages.Count);
        Assert.Equal("Preparing to send email campaign", progressMessages[0]);
        Assert.Equal("Sending email campaign", progressMessages[1]);
        Assert.Equal("Email campaign sending failed", progressMessages[2]);
    }
    
    [Fact]
    public async Task SendCampaignAsync_MissingAccounts_ThrowsArgumentException()
    {
        // Arrange
        var campaign = new EmailCampaign
        {
            Subject = "Test Subject",
            Title = "Test Campaign",
            Message = "<p>Test message</p>",
            SenderEmail = "sender@example.com",
            ReplyEmail = "reply@example.com",
            SenderName = "Test Sender",
            Accounts = new List<EmailAccount>()
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _emailService.SendCampaignAsync(campaign));
        
        Assert.Equal("At least one account is required (Parameter 'campaign.Accounts')", exception.Message);
    }
    
    [Fact]
    public async Task SendSingleAsync_ValidParameters_ReturnsResponse()
    {
        // Arrange
        var responseContent = new EmailResponse
        {
            Id = "campaign-123",
            Status = "success",
            MessagesSent = 1
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
        var result = await _emailService.SendSingleAsync(
            "John",
            "Doe",
            "john@example.com",
            "Test Subject",
            "<p>Test message</p>",
            "sender@example.com",
            "reply@example.com",
            "Test Sender",
            "Test Campaign"
        );
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("campaign-123", result.Id);
        Assert.Equal("success", result.Status);
        Assert.Equal(1, result.MessagesSent);
        
        _mockHttpMessageHandler
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
    public void SendCampaign_SynchronousVersion_CallsAsyncMethod()
    {
        // Arrange
        var campaign = new EmailCampaign
        {
            Subject = "Test Subject",
            Title = "Test Campaign",
            Message = "<p>Test message</p>",
            SenderEmail = "sender@example.com",
            ReplyEmail = "reply@example.com",
            SenderName = "Test Sender",
            Accounts = new List<EmailAccount>
            {
                new EmailAccount
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com"
                }
            }
        };
        
        var responseContent = new EmailResponse
        {
            Id = "campaign-123",
            Status = "success",
            MessagesSent = 1
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
        var result = _emailService.SendCampaign(campaign);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("campaign-123", result.Id);
        Assert.Equal("success", result.Status);
        Assert.Equal(1, result.MessagesSent);
    }
    
    [Fact]
    public async Task SendAsync_WithEmailRequest_ReturnsResponse()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new EmailAccount
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            }
        };
        
        var request = EmailRequest.Create(
            accounts,
            "Test Subject",
            "Test Campaign",
            "<p>Test message</p>",
            "sender@example.com",
            "reply@example.com",
            "Test Sender"
        );
        
        var responseContent = new EmailResponse
        {
            Id = "campaign-123",
            Status = "success",
            MessagesSent = 1
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
        var result = await _emailService.SendAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("campaign-123", result.Id);
        Assert.Equal("success", result.Status);
        Assert.Equal(1, result.MessagesSent);
    }
    
    [Fact]
    public async Task SendAsync_WithEmailRequestProgressTracking_CallsProgressCallback()
    {
        // Arrange
        var accounts = new List<EmailAccount>
        {
            new EmailAccount
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            }
        };
        
        var progressMessages = new List<string>();
        var options = new EmailOptions
        {
            OnProgress = message => progressMessages.Add(message)
        };
        
        var request = EmailRequest.Create(
            accounts,
            "Test Subject",
            "Test Campaign",
            "<p>Test message</p>",
            "sender@example.com",
            "reply@example.com",
            "Test Sender",
            options
        );
        
        var responseContent = new EmailResponse
        {
            Id = "campaign-123",
            Status = "success",
            MessagesSent = 1
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
        var result = await _emailService.SendAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, progressMessages.Count);
        Assert.Equal("Preparing to send email campaign", progressMessages[0]);
        Assert.Equal("Sending email campaign", progressMessages[1]);
        Assert.Equal("Email campaign sent successfully", progressMessages[2]);
    }
}
