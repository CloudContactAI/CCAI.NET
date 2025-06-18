// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text.Json;
using CCAI.NET.SMS;
using Moq;
using Moq.Protected;
using Xunit;

namespace CCAI.NET.Tests.SMS;

public class MMSServiceTests
{
    private readonly Mock<CCAIClient> _mockClient;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly MMSService _mmsService;
    
    public MMSServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        _mockClient = new Mock<CCAIClient>(
            new CCAIConfig { ClientId = "test-client-id", ApiKey = "test-api-key" },
            _httpClient
        );
        
        _mockClient.Setup(c => c.GetClientId()).Returns("test-client-id");
        _mockClient.Setup(c => c.GetApiKey()).Returns("test-api-key");
        
        _mmsService = new MMSService(_mockClient.Object);
    }
    
    [Fact]
    public async Task GetSignedUploadUrlAsync_WithValidInputs_ReturnsSignedUrl()
    {
        // Arrange
        var fileName = "test-image.jpg";
        var fileType = "image/jpeg";
        
        var responseContent = new SignedUrlResponse
        {
            SignedS3Url = "https://s3.amazonaws.com/bucket/signed-url",
            FileKey = "original/file/key"
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
                Content = new StringContent(JsonSerializer.Serialize(responseContent))
            });
        
        // Act
        var result = await _mmsService.GetSignedUploadUrlAsync(fileName, fileType);
        
        // Assert
        Assert.Equal("https://s3.amazonaws.com/bucket/signed-url", result.SignedS3Url);
        Assert.Equal("test-client-id/campaign/test-image.jpg", result.FileKey);
    }
    
    [Fact]
    public async Task GetSignedUploadUrlAsync_WithEmptyFileName_ThrowsArgumentException()
    {
        // Arrange
        var fileName = "";
        var fileType = "image/jpeg";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mmsService.GetSignedUploadUrlAsync(fileName, fileType));
        
        Assert.Equal("fileName", exception.ParamName);
    }
    
    [Fact]
    public async Task GetSignedUploadUrlAsync_WithEmptyFileType_ThrowsArgumentException()
    {
        // Arrange
        var fileName = "test-image.jpg";
        var fileType = "";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mmsService.GetSignedUploadUrlAsync(fileName, fileType));
        
        Assert.Equal("fileType", exception.ParamName);
    }
    
    [Fact]
    public async Task UploadImageToSignedUrlAsync_WithValidInputs_ReturnsTrue()
    {
        // Arrange
        var signedUrl = "https://s3.amazonaws.com/bucket/signed-url";
        var filePath = "test-image.jpg";
        var contentType = "image/jpeg";
        
        // Mock File.Exists
        var mockFile = new Mock<IFile>();
        mockFile.Setup(f => f.Exists(filePath)).Returns(true);
        mockFile.Setup(f => f.ReadAllBytesAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 1, 2, 3 });
        
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });
        
        // Act
        var result = await _mmsService.UploadImageToSignedUrlAsync(signedUrl, filePath, contentType);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task UploadImageToSignedUrlAsync_WithEmptySignedUrl_ThrowsArgumentException()
    {
        // Arrange
        var signedUrl = "";
        var filePath = "test-image.jpg";
        var contentType = "image/jpeg";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mmsService.UploadImageToSignedUrlAsync(signedUrl, filePath, contentType));
        
        Assert.Equal("signedUrl", exception.ParamName);
    }
    
    [Fact]
    public async Task UploadImageToSignedUrlAsync_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var signedUrl = "https://s3.amazonaws.com/bucket/signed-url";
        var filePath = "";
        var contentType = "image/jpeg";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mmsService.UploadImageToSignedUrlAsync(signedUrl, filePath, contentType));
        
        Assert.Equal("filePath", exception.ParamName);
    }
    
    [Fact]
    public async Task UploadImageToSignedUrlAsync_WithEmptyContentType_ThrowsArgumentException()
    {
        // Arrange
        var signedUrl = "https://s3.amazonaws.com/bucket/signed-url";
        var filePath = "test-image.jpg";
        var contentType = "";
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mmsService.UploadImageToSignedUrlAsync(signedUrl, filePath, contentType));
        
        Assert.Equal("contentType", exception.ParamName);
    }
    
    [Fact]
    public async Task SendAsync_WithValidInputs_CallsClientRequestAsync()
    {
        // Arrange
        var pictureFileKey = "test-client-id/campaign/test-image.jpg";
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
                HttpMethod.Post,
                "/clients/test-client-id/campaigns/direct",
                It.IsAny<MMSCampaign>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _mmsService.SendAsync(pictureFileKey, accounts, message, title);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
        Assert.Equal("camp-456", result.CampaignId);
        Assert.Equal(1, result.MessagesSent);
        Assert.Equal("2025-06-06T12:00:00Z", result.Timestamp);
        
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            HttpMethod.Post,
            "/clients/test-client-id/campaigns/direct",
            It.Is<MMSCampaign>(campaign =>
                campaign.PictureFileKey == pictureFileKey &&
                campaign.Message == message &&
                campaign.Title == title &&
                campaign.Accounts.Count() == 1 &&
                campaign.Accounts.First().FirstName == "John" &&
                campaign.Accounts.First().LastName == "Doe" &&
                campaign.Accounts.First().Phone == "+15551234567"),
            It.IsAny<CancellationToken>(),
            It.Is<Dictionary<string, string>>(headers => headers.ContainsKey("ForceNewCampaign") && headers["ForceNewCampaign"] == "true")),
            Times.Once);
    }
    
    [Fact]
    public async Task SendAsync_WithForceNewCampaignFalse_DoesNotAddHeader()
    {
        // Arrange
        var pictureFileKey = "test-client-id/campaign/test-image.jpg";
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
            Status = "sent"
        };
        
        _mockClient
            .Setup(c => c.RequestAsync<SMSResponse>(
                HttpMethod.Post,
                "/clients/test-client-id/campaigns/direct",
                It.IsAny<MMSCampaign>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _mmsService.SendAsync(pictureFileKey, accounts, message, title, forceNewCampaign: false);
        
        // Assert
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            HttpMethod.Post,
            "/clients/test-client-id/campaigns/direct",
            It.IsAny<MMSCampaign>(),
            It.IsAny<CancellationToken>(),
            null),
            Times.Once);
    }
    
    [Fact]
    public async Task SendAsync_WithProgressTracking_NotifiesProgress()
    {
        // Arrange
        var pictureFileKey = "test-client-id/campaign/test-image.jpg";
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
                It.IsAny<MMSCampaign>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _mmsService.SendAsync(pictureFileKey, accounts, message, title, options);
        
        // Assert
        Assert.Equal(3, progressUpdates.Count);
        Assert.Equal("Preparing to send MMS", progressUpdates[0]);
        Assert.Equal("Sending MMS", progressUpdates[1]);
        Assert.Equal("MMS sent successfully", progressUpdates[2]);
    }
    
    [Fact]
    public async Task SendSingleAsync_WithValidInputs_CallsSendAsync()
    {
        // Arrange
        var pictureFileKey = "test-client-id/campaign/test-image.jpg";
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
                It.IsAny<MMSCampaign>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _mmsService.SendSingleAsync(pictureFileKey, firstName, lastName, phone, message, title);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
        
        _mockClient.Verify(c => c.RequestAsync<SMSResponse>(
            HttpMethod.Post,
            "/clients/test-client-id/campaigns/direct",
            It.Is<MMSCampaign>(campaign =>
                campaign.PictureFileKey == pictureFileKey &&
                campaign.Message == message &&
                campaign.Title == title &&
                campaign.Accounts.Count() == 1 &&
                campaign.Accounts.First().FirstName == "Jane" &&
                campaign.Accounts.First().LastName == "Smith" &&
                campaign.Accounts.First().Phone == "+15559876543"),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SendWithImageAsync_WithValidInputs_CompletesWorkflow()
    {
        // Arrange
        var imagePath = "test-image.jpg";
        var contentType = "image/jpeg";
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
        
        // Mock GetSignedUploadUrlAsync
        var signedUrlResponse = new SignedUrlResponse
        {
            SignedS3Url = "https://s3.amazonaws.com/bucket/signed-url",
            FileKey = "test-client-id/campaign/test-image.jpg"
        };
        
        // Mock UploadImageToSignedUrlAsync
        var uploadSuccess = true;
        
        // Mock SendAsync
        var sendResponse = new SMSResponse
        {
            Id = "msg-123",
            Status = "sent",
            CampaignId = "camp-456"
        };
        
        // Setup the mocks
        var mockMmsService = new Mock<MMSService>(_mockClient.Object) { CallBase = true };
        
        mockMmsService
            .Setup(m => m.GetSignedUploadUrlAsync(
                imagePath,
                contentType,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(signedUrlResponse);
        
        mockMmsService
            .Setup(m => m.UploadImageToSignedUrlAsync(
                signedUrlResponse.SignedS3Url,
                imagePath,
                contentType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadSuccess);
        
        mockMmsService
            .Setup(m => m.SendAsync(
                signedUrlResponse.FileKey,
                accounts,
                message,
                title,
                options,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sendResponse);
        
        // Act
        var result = await mockMmsService.Object.SendWithImageAsync(
            imagePath,
            contentType,
            accounts,
            message,
            title,
            options);
        
        // Assert
        Assert.Equal("msg-123", result.Id);
        Assert.Equal("sent", result.Status);
        Assert.Equal("camp-456", result.CampaignId);
        
        Assert.Equal(4, progressUpdates.Count);
        Assert.Equal("Getting signed upload URL", progressUpdates[0]);
        Assert.Equal("Uploading image to S3", progressUpdates[1]);
        Assert.Equal("Image uploaded successfully, sending MMS", progressUpdates[2]);
        
        mockMmsService.Verify(m => m.GetSignedUploadUrlAsync(
            imagePath,
            contentType,
            null,
            true,
            It.IsAny<CancellationToken>()),
            Times.Once);
        
        mockMmsService.Verify(m => m.UploadImageToSignedUrlAsync(
            signedUrlResponse.SignedS3Url,
            imagePath,
            contentType,
            It.IsAny<CancellationToken>()),
            Times.Once);
        
        mockMmsService.Verify(m => m.SendAsync(
            signedUrlResponse.FileKey,
            accounts,
            message,
            title,
            options,
            true,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SendWithImageAsync_WithUploadFailure_ThrowsException()
    {
        // Arrange
        var imagePath = "test-image.jpg";
        var contentType = "image/jpeg";
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
        
        // Mock GetSignedUploadUrlAsync
        var signedUrlResponse = new SignedUrlResponse
        {
            SignedS3Url = "https://s3.amazonaws.com/bucket/signed-url",
            FileKey = "test-client-id/campaign/test-image.jpg"
        };
        
        // Mock UploadImageToSignedUrlAsync to fail
        var uploadSuccess = false;
        
        // Setup the mocks
        var mockMmsService = new Mock<MMSService>(_mockClient.Object) { CallBase = true };
        
        mockMmsService
            .Setup(m => m.GetSignedUploadUrlAsync(
                imagePath,
                contentType,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(signedUrlResponse);
        
        mockMmsService
            .Setup(m => m.UploadImageToSignedUrlAsync(
                signedUrlResponse.SignedS3Url,
                imagePath,
                contentType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadSuccess);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mockMmsService.Object.SendWithImageAsync(
                imagePath,
                contentType,
                accounts,
                message,
                title));
        
        Assert.Equal("Failed to upload image to S3", exception.Message);
    }
}

// Interface for mocking File operations
public interface IFile
{
    bool Exists(string path);
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken);
}
