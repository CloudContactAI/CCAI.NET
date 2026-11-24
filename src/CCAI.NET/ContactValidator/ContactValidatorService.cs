// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.ContactValidator;

/// <summary>
/// Interface for the contact validator service
/// </summary>
public interface IContactValidatorService
{
    /// <summary>
    /// Validate a single email address with Amazon SES (async). Throws <see cref="HttpRequestException"/> with
    /// <c>StatusCode</c> 403 when Email Address Validation is not enabled for the account, and 502 when SES rejects the check.
    /// </summary>
    Task<EmailValidationResult> ValidateEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate multiple email addresses, up to 10 per request (async). Throws <see cref="HttpRequestException"/> with
    /// <c>StatusCode</c> 403 when Email Address Validation is not enabled for the account. An address that SES fails to
    /// check comes back as "invalid", with the reason in <c>Metadata["error"]</c>.
    /// </summary>
    Task<BulkEmailValidationResult> ValidateEmailsAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default);

    /// <summary>Validate a single phone number in E.164 format (async)</summary>
    Task<PhoneValidationResult> ValidatePhoneAsync(string phone, string? countryCode = null, CancellationToken cancellationToken = default);

    /// <summary>Validate multiple phone numbers up to the configured bulk limit (async)</summary>
    Task<BulkPhoneValidationResult> ValidatePhonesAsync(IEnumerable<PhoneInput> phones, CancellationToken cancellationToken = default);

    /// <summary>Validate a single email address (synchronous)</summary>
    EmailValidationResult ValidateEmail(string email);

    /// <summary>Validate multiple email addresses up to the configured bulk limit (synchronous)</summary>
    BulkEmailValidationResult ValidateEmails(IEnumerable<string> emails);

    /// <summary>Validate a single phone number in E.164 format (synchronous)</summary>
    PhoneValidationResult ValidatePhone(string phone, string? countryCode = null);

    /// <summary>Validate multiple phone numbers up to the configured bulk limit (synchronous)</summary>
    BulkPhoneValidationResult ValidatePhones(IEnumerable<PhoneInput> phones);
}

/// <summary>
/// Service for validating email addresses and phone numbers through the CCAI API
/// </summary>
public class ContactValidatorService : IContactValidatorService
{
    private readonly ICCAIClient _client;

    /// <summary>
    /// Create a new ContactValidatorService instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public ContactValidatorService(ICCAIClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Validate a single email address
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with status and metadata</returns>
    public Task<EmailValidationResult> ValidateEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _client.RequestAsync<EmailValidationResult>(HttpMethod.Post, "/v1/contact-validator/email", new { email }, cancellationToken);

    /// <summary>
    /// Validate multiple email addresses (up to the configured bulk limit)
    /// </summary>
    /// <param name="emails">List of email addresses to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk validation results with summary</returns>
    public Task<BulkEmailValidationResult> ValidateEmailsAsync(IEnumerable<string> emails, CancellationToken cancellationToken = default) =>
        _client.RequestAsync<BulkEmailValidationResult>(HttpMethod.Post, "/v1/contact-validator/emails", new { emails }, cancellationToken);

    /// <summary>
    /// Validate a single phone number
    /// </summary>
    /// <param name="phone">Phone number in E.164 format (e.g. +15551234567)</param>
    /// <param name="countryCode">Optional ISO 3166-1 alpha-2 country code (e.g. "US")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with status and metadata</returns>
    public Task<PhoneValidationResult> ValidatePhoneAsync(string phone, string? countryCode = null, CancellationToken cancellationToken = default) =>
        _client.RequestAsync<PhoneValidationResult>(HttpMethod.Post, "/v1/contact-validator/phone", new { phone, countryCode }, cancellationToken);

    /// <summary>
    /// Validate multiple phone numbers (up to the configured bulk limit)
    /// </summary>
    /// <param name="phones">List of phone inputs with optional country codes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk validation results with summary</returns>
    public Task<BulkPhoneValidationResult> ValidatePhonesAsync(IEnumerable<PhoneInput> phones, CancellationToken cancellationToken = default) =>
        _client.RequestAsync<BulkPhoneValidationResult>(HttpMethod.Post, "/v1/contact-validator/phones", new { phones }, cancellationToken);

    /// <inheritdoc/>
    public EmailValidationResult ValidateEmail(string email) =>
        ValidateEmailAsync(email).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public BulkEmailValidationResult ValidateEmails(IEnumerable<string> emails) =>
        ValidateEmailsAsync(emails).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public PhoneValidationResult ValidatePhone(string phone, string? countryCode = null) =>
        ValidatePhoneAsync(phone, countryCode).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public BulkPhoneValidationResult ValidatePhones(IEnumerable<PhoneInput> phones) =>
        ValidatePhonesAsync(phones).GetAwaiter().GetResult();
}
