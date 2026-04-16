// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using CCAI.NET.Contact;
using Moq;
using Xunit;

namespace CCAI.NET.Tests.Contact;

public class ContactServiceTests
{
    private readonly Mock<ICCAIClient> _mockClient;
    private readonly IContactService _contactService;

    public ContactServiceTests()
    {
        _mockClient = new Mock<ICCAIClient>();

        _mockClient.Setup(c => c.GetClientId()).Returns("test-client-id");

        _contactService = new ContactService(_mockClient.Object);
    }

    // ─── SetDoNotTextAsync ────────────────────────────────────────────────

    [Fact]
    public async Task SetDoNotTextAsync_OptOut_CallsPutEndpoint()
    {
        var expectedResponse = new ContactResponse
        {
            DoNotText = true,
            Phone = "+15551234567"
        };

        _mockClient
            .Setup(c => c.RequestAsync<ContactResponse>(
                HttpMethod.Put,
                "/account/do-not-text",
                It.IsAny<SetDoNotTextRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);

        var result = await _contactService.SetDoNotTextAsync(true, phone: "+15551234567");

        Assert.True(result.DoNotText);
        Assert.Equal("+15551234567", result.Phone);

        _mockClient.Verify(c => c.RequestAsync<ContactResponse>(
            HttpMethod.Put,
            "/account/do-not-text",
            It.Is<SetDoNotTextRequest>(r =>
                r.ClientId == "test-client-id" &&
                r.DoNotText == true &&
                r.Phone == "+15551234567" &&
                r.ContactId == null),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task SetDoNotTextAsync_OptIn_SendsFalse()
    {
        var expectedResponse = new ContactResponse
        {
            DoNotText = false,
            Phone = "+15551234567"
        };

        _mockClient
            .Setup(c => c.RequestAsync<ContactResponse>(
                HttpMethod.Put,
                "/account/do-not-text",
                It.IsAny<SetDoNotTextRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedResponse);

        var result = await _contactService.SetDoNotTextAsync(false, phone: "+15551234567");

        Assert.False(result.DoNotText);

        _mockClient.Verify(c => c.RequestAsync<ContactResponse>(
            HttpMethod.Put,
            "/account/do-not-text",
            It.Is<SetDoNotTextRequest>(r => r.DoNotText == false),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task SetDoNotTextAsync_WithContactId_IncludesContactId()
    {
        _mockClient
            .Setup(c => c.RequestAsync<ContactResponse>(
                HttpMethod.Put,
                "/account/do-not-text",
                It.IsAny<SetDoNotTextRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new ContactResponse { DoNotText = true, ContactId = "contact-abc" });

        var result = await _contactService.SetDoNotTextAsync(true, contactId: "contact-abc");

        Assert.Equal("contact-abc", result.ContactId);

        _mockClient.Verify(c => c.RequestAsync<ContactResponse>(
            HttpMethod.Put,
            "/account/do-not-text",
            It.Is<SetDoNotTextRequest>(r =>
                r.ContactId == "contact-abc" &&
                r.Phone == null),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task SetDoNotTextAsync_WithBothContactIdAndPhone_SendsBoth()
    {
        _mockClient
            .Setup(c => c.RequestAsync<ContactResponse>(
                HttpMethod.Put,
                "/account/do-not-text",
                It.IsAny<SetDoNotTextRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new ContactResponse { DoNotText = true });

        await _contactService.SetDoNotTextAsync(true, contactId: "contact-abc", phone: "+15551234567");

        _mockClient.Verify(c => c.RequestAsync<ContactResponse>(
            HttpMethod.Put,
            "/account/do-not-text",
            It.Is<SetDoNotTextRequest>(r =>
                r.ContactId == "contact-abc" &&
                r.Phone == "+15551234567" &&
                r.ClientId == "test-client-id"),
            It.IsAny<CancellationToken>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task SetDoNotTextAsync_ApiError_ThrowsHttpRequestException()
    {
        _mockClient
            .Setup(c => c.RequestAsync<ContactResponse>(
                HttpMethod.Put,
                "/account/do-not-text",
                It.IsAny<SetDoNotTextRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new HttpRequestException("Unauthorized"));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _contactService.SetDoNotTextAsync(true, phone: "+15551234567"));
    }

    // ─── SetDoNotText (sync wrapper) ──────────────────────────────────────

    [Fact]
    public void SetDoNotText_Sync_ReturnsResult()
    {
        _mockClient
            .Setup(c => c.RequestAsync<ContactResponse>(
                HttpMethod.Put,
                "/account/do-not-text",
                It.IsAny<SetDoNotTextRequest>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new ContactResponse { DoNotText = true, Phone = "+15551234567" });

        var result = _contactService.SetDoNotText(true, phone: "+15551234567");

        Assert.True(result.DoNotText);
        Assert.Equal("+15551234567", result.Phone);
    }

    // ─── CCAIClient.Contact registration ─────────────────────────────────

    [Fact]
    public void CCAIClient_HasContactService_NotNull()
    {
        using var client = new CCAIClient(new CCAIConfig
        {
            ClientId = "test-client-id",
            ApiKey = "test-api-key"
        });

        Assert.NotNull(client.Contact);
    }
}
