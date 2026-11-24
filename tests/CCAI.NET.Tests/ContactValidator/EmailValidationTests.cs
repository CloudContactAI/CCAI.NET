// Copyright (c) 2026 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Text;
using System.Text.Json;
using CCAI.NET.ContactValidator;
using Xunit;

namespace CCAI.NET.Tests.ContactValidator;

/// <summary>
/// Email validation with Amazon SES. The responses in Fixtures were recorded from the contact-validator API.
/// </summary>
public class EmailValidationTests
{
    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public RecordingHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        public HttpMethod? Method { get; private set; }
        public string? Url { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Url = request.RequestUri?.ToString();
            RequestBody = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            };
        }
    }

    private static string Recorded(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ContactValidator", "Fixtures", $"{name}.json"));

    private static (IContactValidatorService Service, RecordingHandler Handler) ServiceReplying(HttpStatusCode status, string body)
    {
        var handler = new RecordingHandler(status, body);
        var client = new CCAIClient(new CCAIConfig { ClientId = "test-client-id", ApiKey = "test-api-key" }, new HttpClient(handler));
        return (client.ContactValidator, handler);
    }

    private static async Task<EmailValidationResult> ValidateEmail(string fixture, string email = "jane@example.com")
    {
        var (service, _) = ServiceReplying(HttpStatusCode.OK, Recorded(fixture));
        return await service.ValidateEmailAsync(email);
    }

    [Fact]
    public async Task ValidateEmailAsync_PostsTheAddressToTheEmailEndpoint()
    {
        var (service, handler) = ServiceReplying(HttpStatusCode.OK, Recorded("email-valid"));

        await service.ValidateEmailAsync("jane@example.com");

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.EndsWith("/v1/contact-validator/email", handler.Url);
        Assert.Contains("\"email\":\"jane@example.com\"", handler.RequestBody);
    }

    [Fact]
    public async Task ValidateEmailAsync_ReturnsTheVerdictTheSixChecksTheDateAndReused()
    {
        var result = await ValidateEmail("email-valid");

        Assert.Equal("valid", result.Status);
        Assert.Equal("jane@example.com", result.ContactField);
        Assert.True(result.Metadata!["safe_to_send"].GetBoolean());
        Assert.Equal("valid", result.Metadata["ai_verdict"].GetString());
        Assert.Equal("HIGH", result.Verdict);
        Assert.NotNull(result.Checks);
        Assert.Equal("HIGH", result.Checks!.HasValidSyntax);
        Assert.Equal("HIGH", result.Checks.HasValidDnsRecords);
        Assert.Equal("HIGH", result.Checks.MailboxExists);
        Assert.Equal("LOW", result.Checks.IsRoleAddress);
        Assert.Equal("LOW", result.Checks.IsDisposable);
        Assert.Equal("LOW", result.Checks.IsRandomInput);
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 15, 2, 11, TimeSpan.Zero), result.ValidatedAt);
        Assert.False(result.Reused);
    }

    [Fact]
    public async Task ValidateEmailAsync_TheFieldsAreAlsoInTheMetadata()
    {
        var result = await ValidateEmail("email-valid");

        Assert.Equal("HIGH", result.Metadata!["verdict"].GetString());
        Assert.Equal("2026-11-13T15:02:11Z", result.Metadata["validatedAt"].GetString());
        Assert.False(result.Metadata["reused"].GetBoolean());
        Assert.Equal("HIGH", result.Metadata["checks"].GetProperty("mailboxExists").GetString());
    }

    [Fact]
    public async Task ValidateEmailAsync_SaysWhenTheResultIsASavedCheck()
    {
        var result = await ValidateEmail("email-reused");
        Assert.True(result.Reused);
    }

    [Fact]
    public async Task ValidateEmailAsync_ADisposableAddressIsRisky()
    {
        var result = await ValidateEmail("email-risky-disposable", "temp@mailinator.com");

        Assert.Equal("risky", result.Status);
        Assert.False(result.Metadata!["safe_to_send"].GetBoolean());
        Assert.Equal("HIGH", result.Checks!.IsDisposable);
    }

    [Fact]
    public async Task ValidateEmailAsync_ALowAddressIsInvalid()
    {
        var result = await ValidateEmail("email-invalid-low", "nobody@no-such-domain-example.xyz");

        Assert.Equal("invalid", result.Status);
        Assert.Equal("LOW", result.Verdict);
        Assert.Equal("LOW", result.Checks!.HasValidDnsRecords);
    }

    [Fact]
    public async Task ValidateEmailAsync_AnAddressRejectedBeforeAskingSesHasUnknownChecksNoDateAndTheReason()
    {
        var result = await ValidateEmail("email-rejected-format", "not-an-email");

        Assert.Equal("invalid", result.Status);
        Assert.Equal("UNKNOWN", result.Verdict);
        Assert.All(
            new[] { result.Checks!.HasValidSyntax, result.Checks.HasValidDnsRecords, result.Checks.MailboxExists,
                    result.Checks.IsRoleAddress, result.Checks.IsDisposable, result.Checks.IsRandomInput },
            verdict => Assert.Equal("UNKNOWN", verdict));
        Assert.Null(result.ValidatedAt);
        Assert.Equal("Invalid email format", result.Metadata!["error"].GetString());
    }

    [Fact]
    public async Task ValidateEmailAsync_AResponseWithoutTheNewFieldsIsReadAsBefore()
    {
        var result = await ValidateEmail("email-legacy");

        Assert.Equal("valid", result.Status);
        Assert.True(result.Metadata!["safe_to_send"].GetBoolean());
        Assert.Equal("Mode: Sandbox", result.Metadata["ai_verdict"].GetString());
        Assert.Null(result.Verdict);
        Assert.Null(result.Checks);
        Assert.Null(result.ValidatedAt);
        Assert.Null(result.Reused);
    }

    [Fact]
    public void TheNewFieldsAreNotPartOfTheJsonOfTheResult()
    {
        var result = new EmailValidationResult { ContactField = "a@b.com", Type = "email", Status = "valid" };

        var json = JsonSerializer.Serialize(result);

        Assert.DoesNotContain("Verdict", json);
        Assert.DoesNotContain("Checks", json);
        Assert.DoesNotContain("ValidatedAt", json);
        Assert.DoesNotContain("Reused", json);
    }

    [Fact]
    public async Task ValidateEmailAsync_FailsWith403WhenEmailValidationIsNotEnabledForTheAccount()
    {
        var (service, _) = ServiceReplying(HttpStatusCode.Forbidden, Recorded("error-403"));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => service.ValidateEmailAsync("jane@example.com"));

        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
    }

    [Fact]
    public async Task ValidateEmailAsync_FailsWith502WhenSesRejectsTheCheck()
    {
        var (service, _) = ServiceReplying(HttpStatusCode.BadGateway, Recorded("error-502"));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => service.ValidateEmailAsync("jane@example.com"));

        Assert.Equal(HttpStatusCode.BadGateway, error.StatusCode);
    }

    [Fact]
    public async Task ValidateEmailsAsync_PostsTheAddressesAndReturnsTheNewFieldsOnEveryResult()
    {
        var (service, handler) = ServiceReplying(HttpStatusCode.OK, Recorded("emails-bulk"));
        var emails = new[] { "jane@example.com", "temp@mailinator.com", "bad@example.com" };

        var bulk = await service.ValidateEmailsAsync(emails);

        Assert.EndsWith("/v1/contact-validator/emails", handler.Url);
        Assert.Contains("\"emails\":[\"jane@example.com\",\"temp@mailinator.com\",\"bad@example.com\"]", handler.RequestBody);
        Assert.Equal(3, bulk.Summary.Total);
        Assert.Equal(1, bulk.Summary.Valid);
        Assert.Equal(1, bulk.Summary.Invalid);
        Assert.Equal(1, bulk.Summary.Risky);
        var (reused, risky, failed) = (bulk.Results[0], bulk.Results[1], bulk.Results[2]);
        Assert.True(reused.Reused);
        Assert.Equal("HIGH", reused.Verdict);
        Assert.Equal("MEDIUM", risky.Verdict);
        Assert.Equal("HIGH", risky.Checks!.IsDisposable);
        Assert.Equal(new DateTimeOffset(2026, 11, 13, 15, 5, 40, TimeSpan.Zero), risky.ValidatedAt);
        // An address SES failed to check comes back invalid with the reason, and none of the new fields
        Assert.Equal("invalid", failed.Status);
        Assert.Equal("The domain is not allowed", failed.Metadata!["error"].GetString());
        Assert.Null(failed.Verdict);
        Assert.Null(failed.Checks);
    }

    [Fact]
    public async Task ValidateEmailsAsync_FailsWith403WhenEmailValidationIsNotEnabledForTheAccount()
    {
        var (service, _) = ServiceReplying(HttpStatusCode.Forbidden, Recorded("error-403"));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => service.ValidateEmailsAsync(new[] { "jane@example.com" }));

        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
    }
}
