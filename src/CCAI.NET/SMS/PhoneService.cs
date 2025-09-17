// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.SMS;

/// <summary>
/// Phone service for managing client phones through the CCAI API
/// </summary>
public class PhoneService
{
    private readonly CCAIClient _client;
    
    /// <summary>
    /// Create a new Phone service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public PhoneService(CCAIClient client)
    {
        _client = client;
    }
    
    /// <summary>
    /// List all phones for the client
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of phones</returns>
    public async Task<List<Phone>> ListAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = $"/v1/client/{_client.GetClientId()}/phones";
        return await _client.RequestAsync<List<Phone>>(HttpMethod.Get, endpoint, null, cancellationToken);
    }
    
    /// <summary>
    /// Get a specific phone by phone ID
    /// </summary>
    /// <param name="phoneId">Phone ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Phone details</returns>
    public async Task<Phone> GetAsync(long phoneId, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/v1/client/{_client.GetClientId()}/phones/{phoneId}";
        return await _client.RequestAsync<Phone>(HttpMethod.Get, endpoint, null, cancellationToken);
    }
    
    /// <summary>
    /// Delete a phone by ID
    /// </summary>
    /// <param name="phoneId">Phone ID</param>
    /// <param name="release">Whether to release the phone number (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    public async Task DeleteAsync(long phoneId, bool release = false, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/v1/client/phone/{phoneId}?release={release}";
        await _client.RequestWithoutResponseAsync(HttpMethod.Delete, endpoint, null, cancellationToken);
    }
    
    /// <summary>
    /// List all phones for the client (synchronous version)
    /// </summary>
    /// <returns>List of phones</returns>
    public List<Phone> List()
    {
        return ListAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Get a specific phone by phone ID (synchronous version)
    /// </summary>
    /// <param name="phoneId">Phone ID</param>
    /// <returns>Phone details</returns>
    public Phone Get(long phoneId)
    {
        return GetAsync(phoneId).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Delete a phone by ID (synchronous version)
    /// </summary>
    /// <param name="phoneId">Phone ID</param>
    /// <param name="release">Whether to release the phone number (default: false)</param>
    public void Delete(long phoneId, bool release = false)
    {
        DeleteAsync(phoneId, release).GetAwaiter().GetResult();
    }
}
