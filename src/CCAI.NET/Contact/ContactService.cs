// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.Contact;

/// <summary>
/// Interface for service managing contact preferences through the CCAI API
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Set the do-not-text preference for a contact (opt-out or opt-in)
    /// </summary>
    Task<ContactResponse> SetDoNotTextAsync(
        bool doNotText,
        string? contactId = null,
        string? phone = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the do-not-text preference for a contact (synchronous version)
    /// </summary>
    ContactResponse SetDoNotText(
        bool doNotText,
        string? contactId = null,
        string? phone = null);
}

/// <summary>
/// Service for managing contact preferences through the CCAI API
/// </summary>
public class ContactService : IContactService
{
    private readonly ICCAIClient _client;

    /// <summary>
    /// Create a new Contact service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public ContactService(ICCAIClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Set the do-not-text preference for a contact (opt-out or opt-in)
    /// </summary>
    /// <param name="doNotText">True to opt out, false to opt in</param>
    /// <param name="contactId">Optional contact ID</param>
    /// <param name="phone">Optional phone number in E.164 format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated contact preferences</returns>
    /// <exception cref="InvalidOperationException">If the API request fails</exception>
    public async Task<ContactResponse> SetDoNotTextAsync(
        bool doNotText,
        string? contactId = null,
        string? phone = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new SetDoNotTextRequest
        {
            ClientId = _client.GetClientId(),
            DoNotText = doNotText,
            ContactId = contactId,
            Phone = phone
        };

        return await _client.RequestAsync<ContactResponse>(
            HttpMethod.Put,
            "/account/do-not-text",
            payload,
            cancellationToken);
    }

    /// <summary>
    /// Set the do-not-text preference for a contact (synchronous version)
    /// </summary>
    /// <param name="doNotText">True to opt out, false to opt in</param>
    /// <param name="contactId">Optional contact ID</param>
    /// <param name="phone">Optional phone number in E.164 format</param>
    /// <returns>Updated contact preferences</returns>
    public ContactResponse SetDoNotText(
        bool doNotText,
        string? contactId = null,
        string? phone = null)
    {
        return SetDoNotTextAsync(doNotText, contactId, phone)
            .GetAwaiter()
            .GetResult();
    }
}
