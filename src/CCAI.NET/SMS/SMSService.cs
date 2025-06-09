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
    /// Send an SMS message to one or more recipients
    /// </summary>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    public async Task<SMSResponse> SendAsync(
        IEnumerable<Account> accounts,
        string message,
        string title,
        SMSOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        var accountsList = accounts?.ToList() ?? throw new ArgumentNullException(nameof(accounts));
        
        if (accountsList.Count == 0)
        {
            throw new ArgumentException("At least one account is required", nameof(accounts));
        }
        
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("Message is required", nameof(message));
        }
        
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentException("Campaign title is required", nameof(title));
        }
        
        // Create options if not provided
        options ??= new SMSOptions();
        
        // Notify progress if callback provided
        options.NotifyProgress("Preparing to send SMS");
        
        // Prepare the endpoint and data
        var endpoint = $"/clients/{_client.GetClientId()}/campaigns/direct";
        
        var campaignData = new SMSCampaign
        {
            Accounts = accountsList,
            Message = message,
            Title = title
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
            
            // Make the API request
            var response = await _client.RequestAsync<SMSResponse>(
                HttpMethod.Post,
                endpoint,
                campaignData,
                linkedCts.Token);
            
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
    /// Send a single SMS message to one recipient
    /// </summary>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="phone">Recipient's phone number (E.164 format)</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public Task<SMSResponse> SendSingleAsync(
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        SMSOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone
        };
        
        return SendAsync(new[] { account }, message, title, options, cancellationToken);
    }
    
    /// <summary>
    /// Send an SMS message to one or more recipients (synchronous version)
    /// </summary>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <returns>API response</returns>
    public SMSResponse Send(
        IEnumerable<Account> accounts,
        string message,
        string title,
        SMSOptions? options = null)
    {
        return SendAsync(accounts, message, title, options).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Send a single SMS message to one recipient (synchronous version)
    /// </summary>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="phone">Recipient's phone number (E.164 format)</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the SMS send operation</param>
    /// <returns>API response</returns>
    public SMSResponse SendSingle(
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        SMSOptions? options = null)
    {
        return SendSingleAsync(firstName, lastName, phone, message, title, options).GetAwaiter().GetResult();
    }
}
