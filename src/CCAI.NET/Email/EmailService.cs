// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.Email;

/// <summary>
/// Email service for sending campaigns through the CCAI API
/// </summary>
public class EmailService
{
    private readonly CCAIClient _client;
    
    /// <summary>
    /// Create a new Email service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public EmailService(CCAIClient client)
    {
        _client = client;
    }
    
    /// <summary>
    /// Get the email base URL with API version
    /// </summary>
    private string GetEmailBaseUrl() => $"{_client.GetEmailBaseUrl()}/api/v1";
    
    /// <summary>
    /// Send an email campaign to one or more recipients
    /// </summary>
    /// <param name="campaign">Email campaign configuration</param>
    /// <param name="options">Optional settings for the email send operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    public async Task<EmailResponse> SendCampaignAsync(
        EmailCampaign campaign,
        EmailOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (campaign.Accounts == null || campaign.Accounts.Count == 0)
        {
            throw new ArgumentException("At least one account is required", nameof(campaign.Accounts));
        }
        
        if (string.IsNullOrEmpty(campaign.Subject))
        {
            throw new ArgumentException("Subject is required", nameof(campaign.Subject));
        }
        
        if (string.IsNullOrEmpty(campaign.Title))
        {
            throw new ArgumentException("Campaign title is required", nameof(campaign.Title));
        }
        
        if (string.IsNullOrEmpty(campaign.Message))
        {
            throw new ArgumentException("Message content is required", nameof(campaign.Message));
        }
        
        if (string.IsNullOrEmpty(campaign.SenderEmail))
        {
            throw new ArgumentException("Sender email is required", nameof(campaign.SenderEmail));
        }
        
        if (string.IsNullOrEmpty(campaign.ReplyEmail))
        {
            throw new ArgumentException("Reply email is required", nameof(campaign.ReplyEmail));
        }
        
        if (string.IsNullOrEmpty(campaign.SenderName))
        {
            throw new ArgumentException("Sender name is required", nameof(campaign.SenderName));
        }
        
        // Validate each account has the required fields
        for (var i = 0; i < campaign.Accounts.Count; i++)
        {
            var account = campaign.Accounts[i];
            
            if (string.IsNullOrEmpty(account.FirstName))
            {
                throw new ArgumentException($"First name is required for account at index {i}", nameof(campaign.Accounts));
            }
            
            if (string.IsNullOrEmpty(account.LastName))
            {
                throw new ArgumentException($"Last name is required for account at index {i}", nameof(campaign.Accounts));
            }
            
            if (string.IsNullOrEmpty(account.Email))
            {
                throw new ArgumentException($"Email is required for account at index {i}", nameof(campaign.Accounts));
            }
        }
        
        // Create options if not provided
        options ??= new EmailOptions();
        
        // Notify progress if callback provided
        options.NotifyProgress("Preparing to send email campaign");
        
        // Prepare the endpoint
        var endpoint = "/campaigns";
        
        try
        {
            // Notify progress if callback provided
            options.NotifyProgress("Sending email campaign");
            
            // Create a cancellation token with timeout if specified
            using var timeoutCts = options.Timeout.HasValue
                ? new CancellationTokenSource(TimeSpan.FromSeconds(options.Timeout.Value))
                : new CancellationTokenSource();
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutCts.Token,
                cancellationToken);
            
            // Make the API request to the email campaigns API
            var headers = new Dictionary<string, string>
            {
                { "AccountId", _client.GetClientId() },
                { "ClientId", _client.GetClientId() }
            };
            
            var response = await _client.CustomRequestAsync<EmailResponse>(
                HttpMethod.Post,
                endpoint,
                campaign,
                GetEmailBaseUrl(),
                linkedCts.Token,
                headers);
            
            // Notify progress if callback provided
            options.NotifyProgress("Email campaign sent successfully");
            
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Notify progress if callback provided
            options.NotifyProgress("Email campaign sending cancelled");
            throw;
        }
        catch (OperationCanceledException)
        {
            // Notify progress if callback provided
            options.NotifyProgress("Email campaign sending timed out");
            throw new TimeoutException("Email campaign sending timed out");
        }
        catch (Exception ex)
        {
            // Notify progress if callback provided
            options.NotifyProgress("Email campaign sending failed");
            
            throw new InvalidOperationException($"Failed to send email campaign: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Send a single email to one recipient
    /// </summary>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="email">Recipient's email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="message">HTML message content</param>
    /// <param name="senderEmail">Sender's email address</param>
    /// <param name="replyEmail">Reply-to email address</param>
    /// <param name="senderName">Sender's name</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the email send operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public Task<EmailResponse> SendSingleAsync(
        string firstName,
        string lastName,
        string email,
        string subject,
        string message,
        string senderEmail,
        string replyEmail,
        string senderName,
        string title,
        EmailOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var account = new EmailAccount
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };
        
        var campaign = new EmailCampaign
        {
            Subject = subject,
            Title = title,
            Message = message,
            SenderEmail = senderEmail,
            ReplyEmail = replyEmail,
            SenderName = senderName,
            Accounts = new List<EmailAccount> { account },
            CampaignType = "EMAIL",
            AddToList = "noList",
            ContactInput = "accounts",
            FromType = "single",
            Senders = new List<object>()
        };
        
        return SendCampaignAsync(campaign, options, cancellationToken);
    }
    
    /// <summary>
    /// Send an email campaign to one or more recipients (synchronous version)
    /// </summary>
    /// <param name="campaign">Email campaign configuration</param>
    /// <param name="options">Optional settings for the email send operation</param>
    /// <returns>API response</returns>
    public EmailResponse SendCampaign(
        EmailCampaign campaign,
        EmailOptions? options = null)
    {
        return SendCampaignAsync(campaign, options).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Send a single email to one recipient (synchronous version)
    /// </summary>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="email">Recipient's email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="message">HTML message content</param>
    /// <param name="senderEmail">Sender's email address</param>
    /// <param name="replyEmail">Reply-to email address</param>
    /// <param name="senderName">Sender's name</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the email send operation</param>
    /// <returns>API response</returns>
    public EmailResponse SendSingle(
        string firstName,
        string lastName,
        string email,
        string subject,
        string message,
        string senderEmail,
        string replyEmail,
        string senderName,
        string title,
        EmailOptions? options = null)
    {
        return SendSingleAsync(firstName, lastName, email, subject, message, senderEmail, replyEmail, senderName, title, options)
            .GetAwaiter()
            .GetResult();
    }
}
