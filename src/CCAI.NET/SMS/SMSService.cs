// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.SMS;

/// <summary>
/// SMS service for sending messages through the CCAI API
/// </summary>
public class SMSService
{
    private readonly CCAIClient _client;
    
    /// <summary>
    /// Create a new SMS service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public SMSService(CCAIClient client)
    {
        _client = client;
    }
    
    /// <summary>
    /// Send an SMS message using a request object
    /// </summary>
    /// <param name="request">SMS request containing all parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    public async Task<SMSResponse> SendAsync(SMSRequest request, CancellationToken cancellationToken = default)
    {
        // Validate inputs
        var accountsList = request.Accounts?.ToList() ?? throw new ArgumentNullException(nameof(request.Accounts));
        
        if (accountsList.Count == 0)
        {
            throw new ArgumentException("At least one account is required", nameof(request.Accounts));
        }
        
        if (string.IsNullOrEmpty(request.Message))
        {
            throw new ArgumentException("Message is required", nameof(request.Message));
        }
        
        if (string.IsNullOrEmpty(request.Title))
        {
            throw new ArgumentException("Campaign title is required", nameof(request.Title));
        }
        
        // Create options if not provided
        var options = request.Options ?? new SMSOptions();
        
        // Notify progress if callback provided
        options.NotifyProgress("Preparing to send SMS");
        
        // Prepare the endpoint and data
        var endpoint = $"/clients/{_client.GetClientId()}/campaigns/direct";
        
        var campaignData = new SMSCampaign
        {
            Accounts = accountsList,
            Message = request.Message,
            Title = request.Title,
            SenderPhone = request.SenderPhone
        };
        
        try
        {
            // Notify progress if callback provided
            options.NotifyProgress("Sending SMS");
            
            // Create a cancellation token with timeout if specified
            using var timeoutCts = options.Timeout.HasValue
                ? new CancellationTokenSource(TimeSpan.FromSeconds(options.Timeout.Value))
                : new CancellationTokenSource();
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutCts.Token,
                cancellationToken);
            
            // Make the API request with ForceNewCampaign header
            var headers = new Dictionary<string, string>
            {
                { "ForceNewCampaign", "false" }
            };
            
            var response = await _client.RequestAsync<SMSResponse>(
                HttpMethod.Post,
                endpoint,
                campaignData,
                linkedCts.Token,
                headers);
            
            // Notify progress if callback provided
            options.NotifyProgress("SMS sent successfully");
            
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Notify progress if callback provided
            options.NotifyProgress("SMS sending cancelled");
            throw;
        }
        catch (OperationCanceledException)
        {
            // Notify progress if callback provided
            options.NotifyProgress("SMS sending timed out");
            throw new TimeoutException("SMS sending timed out");
        }
        catch (Exception ex)
        {
            // Notify progress if callback provided
            options.NotifyProgress("SMS sending failed");
            
            throw new InvalidOperationException($"Failed to send SMS: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Send an SMS message to one or more recipients
    /// </summary>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="customData">Custom data to be included with the message</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <param name="senderPhone">Optional sender phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    public Task<SMSResponse> SendAsync(
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? customData = null,
        SMSOptions? options = null,
        string? senderPhone = null,
        CancellationToken cancellationToken = default)
    {
        var request = SMSRequest.Create(accounts, message, title, customData, options, senderPhone);
        return SendAsync(request, cancellationToken);
    }
    
    /// <summary>
    /// Send a single SMS message to one recipient
    /// </summary>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="phone">Recipient's phone number (E.164 format)</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="customAccountId">Custom id for the recipient account, used to identify the user externally</param>
    /// <param name="customData">Custom data to be included with the message</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <param name="senderPhone">Optional sender phone number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public Task<SMSResponse> SendSingleAsync(
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        string? customAccountId = null,
        string? customData = null,
        SMSOptions? options = null,
        string? senderPhone = null,
        CancellationToken cancellationToken = default)
    {
        var request = SMSRequest.CreateSingle(firstName, lastName, phone, message, title, customAccountId, customData, options, senderPhone);
        return SendAsync(request, cancellationToken);
    }
    
    /// <summary>
    /// Send an SMS message using a request object (synchronous version)
    /// </summary>
    /// <param name="request">SMS request containing all parameters</param>
    /// <returns>API response</returns>
    public SMSResponse Send(SMSRequest request)
    {
        return SendAsync(request).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Send an SMS message to one or more recipients (synchronous version)
    /// </summary>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="customData">Custom data to be included with the message</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <param name="senderPhone">Optional sender phone number</param>
    /// <returns>API response</returns>
    public SMSResponse Send(
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? customData = null,
        SMSOptions? options = null,
        string? senderPhone = null)
    {
        return SendAsync(accounts, message, title, customData, options, senderPhone).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Send a single SMS message to one recipient (synchronous version)
    /// </summary>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="phone">Recipient's phone number (E.164 format)</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="customAccountId">Custom id for the recipient account, used to identify the user externally</param>
    /// <param name="customData">Custom data to be included with the message</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <param name="senderPhone">Optional sender phone number</param>
    /// <returns>API response</returns>
    public SMSResponse SendSingle(
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        string? customAccountId = null,
        string? customData = null,
        SMSOptions? options = null,
        string? senderPhone = null)
    {
        return SendSingleAsync(firstName, lastName, phone, message, title, customAccountId, customData, options, senderPhone).GetAwaiter().GetResult();
    }
}
