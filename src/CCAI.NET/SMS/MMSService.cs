// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

namespace CCAI.NET.SMS;

/// <summary>
/// Interface for MMS service for sending multimedia messages through the CCAI API
/// </summary>
public interface IMMSService
{
    /// <summary>
    /// Get a signed S3 URL to upload an image file
    /// </summary>
    Task<SignedUrlResponse> GetSignedUploadUrlAsync(
        string fileName,
        string fileType,
        string? fileBasePath = null,
        bool publicFile = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file has already been uploaded to S3
    /// </summary>
    Task<StoredUrlResponse> CheckFileUploadedAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file has already been uploaded to S3 (synchronous version)
    /// </summary>
    StoredUrlResponse CheckFileUploaded(string fileKey);

    /// <summary>
    /// Upload an image file to a signed S3 URL
    /// </summary>
    Task<bool> UploadImageToSignedUrlAsync(
        string signedUrl,
        string filePath,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send an MMS message to one or more recipients
    /// </summary>
    Task<SMSResponse> SendAsync(
        string pictureFileKey,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a single MMS message to one recipient
    /// </summary>
    Task<SMSResponse> SendSingleAsync(
        string pictureFileKey,
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        string? customData = null,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete MMS workflow: get signed URL, upload image, and send MMS
    /// </summary>
    Task<SMSResponse> SendWithImageAsync(
        string imagePath,
        string contentType,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send an MMS message to one or more recipients (synchronous version)
    /// </summary>
    SMSResponse Send(
        string pictureFileKey,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true);

    /// <summary>
    /// Send a single MMS message to one recipient (synchronous version)
    /// </summary>
    SMSResponse SendSingle(
        string pictureFileKey,
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        string? customData = null,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true);

    /// <summary>
    /// Complete MMS workflow: get signed URL, upload image, and send MMS (synchronous version)
    /// </summary>
    SMSResponse SendWithImage(
        string imagePath,
        string contentType,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true);
}

/// <summary>
/// MMS service for sending multimedia messages through the CCAI API
/// </summary>
public class MMSService : IMMSService
{
    private readonly ICCAIClient _client;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Create a new MMS service instance
    /// </summary>
    /// <param name="client">The parent CCAI client</param>
    public MMSService(ICCAIClient client)
    {
        _client = client;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", client.GetApiKey());
    }
    
    /// <summary>
    /// Get a signed S3 URL to upload an image file
    /// </summary>
    /// <param name="fileName">Name of the file to upload</param>
    /// <param name="fileType">MIME type of the file</param>
    /// <param name="fileBasePath">Base path for the file in S3 (default: clientId/campaign)</param>
    /// <param name="publicFile">Whether the file should be public (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response containing the signed URL and file key</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    /// <exception cref="InvalidOperationException">If the API request fails</exception>
    public async Task<SignedUrlResponse> GetSignedUploadUrlAsync(
        string fileName,
        string fileType,
        string? fileBasePath = null,
        bool publicFile = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name is required", nameof(fileName));
        }
        
        if (string.IsNullOrEmpty(fileType))
        {
            throw new ArgumentException("File type is required", nameof(fileType));
        }
        
        // Use default fileBasePath if not provided
        fileBasePath ??= $"{_client.GetClientId()}/campaign";

        // Define fileKey explicitly as clientId/campaign/filename
        var fileKey = $"{_client.GetClientId()}/campaign/{fileName}";

        var data = new
        {
            fileName,
            fileType,
            fileBasePath,
            publicFile
        };

        try
        {
            var uploadUrl = $"{_client.GetFilesBaseUrl()}/upload/url";
            var response = await _httpClient.PostAsJsonAsync(uploadUrl, data, cancellationToken);

            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<SignedUrlResponse>(
                cancellationToken: cancellationToken);

            if (responseData == null || string.IsNullOrEmpty(responseData.SignedS3Url))
            {
                throw new InvalidOperationException("Invalid response from upload URL API");
            }

            // Override the fileKey with our explicitly defined one
            responseData.FileKey = fileKey;

            return responseData;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to get signed upload URL: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if a file has already been uploaded to S3
    /// </summary>
    /// <param name="fileKey">The S3 file key to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>StoredUrlResponse with the stored URL, or empty StoredUrl if not found</returns>
    public async Task<StoredUrlResponse> CheckFileUploadedAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"/clients/{_client.GetClientId()}/storedUrl?fileKey={Uri.EscapeDataString(fileKey)}";
            return await _client.RequestAsync<StoredUrlResponse>(HttpMethod.Get, endpoint, null, cancellationToken);
        }
        catch
        {
            return new StoredUrlResponse { StoredUrl = string.Empty };
        }
    }

    /// <summary>
    /// Check if a file has already been uploaded to S3 (synchronous version)
    /// </summary>
    /// <param name="fileKey">The S3 file key to check</param>
    /// <returns>StoredUrlResponse with the stored URL, or empty StoredUrl if not found</returns>
    public StoredUrlResponse CheckFileUploaded(string fileKey)
    {
        return CheckFileUploadedAsync(fileKey).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Compute the MD5 hash of a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>Hex-encoded MD5 hash</returns>
    private static string ComputeMD5(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
    
    /// <summary>
    /// Upload an image file to a signed S3 URL
    /// </summary>
    /// <param name="signedUrl">The signed S3 URL to upload to</param>
    /// <param name="filePath">Path to the file to upload</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if upload was successful</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    /// <exception cref="InvalidOperationException">If the file upload fails</exception>
    public async Task<bool> UploadImageToSignedUrlAsync(
        string signedUrl,
        string filePath,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(signedUrl))
        {
            throw new ArgumentException("Signed URL is required", nameof(signedUrl));
        }
        
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path is required", nameof(filePath));
        }
        
        if (!File.Exists(filePath))
        {
            throw new ArgumentException($"File does not exist: {filePath}", nameof(filePath));
        }
        
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentException("Content type is required", nameof(contentType));
        }
        
        try
        {
            var fileContent = await File.ReadAllBytesAsync(filePath, cancellationToken);
            
            // Use a separate HttpClient for S3 upload (without authorization headers)
            using var s3HttpClient = new HttpClient();
            using var content = new ByteArrayContent(fileContent);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            
            var request = new HttpRequestMessage(HttpMethod.Put, signedUrl)
            {
                Content = content
            };
            
            var response = await s3HttpClient.SendAsync(request, cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Send an MMS message to one or more recipients
    /// </summary>
    /// <param name="pictureFileKey">S3 file key for the image</param>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the MMS send operation</param>
    /// <param name="forceNewCampaign">Whether to force a new campaign (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    public async Task<SMSResponse> SendAsync(
        string pictureFileKey,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(pictureFileKey))
        {
            throw new ArgumentException("Picture file key is required", nameof(pictureFileKey));
        }
        
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
        options.NotifyProgress("Preparing to send MMS");
        
        // Prepare the endpoint and data
        var endpoint = $"/clients/{_client.GetClientId()}/campaigns/direct";
        
        var campaignData = new MMSCampaign
        {
            PictureFileKey = pictureFileKey,
            Accounts = accountsList,
            Message = message,
            Title = title,
            SenderPhone = string.IsNullOrEmpty(senderPhone) ? null : senderPhone
        };
        
        try
        {
            // Notify progress if callback provided
            options.NotifyProgress("Sending MMS");
            
            // Create a cancellation token with timeout if specified
            using var timeoutCts = options.Timeout.HasValue
                ? new CancellationTokenSource(TimeSpan.FromSeconds(options.Timeout.Value))
                : new CancellationTokenSource();
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutCts.Token,
                cancellationToken);
            
            // Set up headers for force new campaign if needed
            var headers = forceNewCampaign
                ? new Dictionary<string, string> { { "ForceNewCampaign", "true" } }
                : null;
            
            // Make the API request
            var response = await _client.RequestAsync<SMSResponse>(
                HttpMethod.Post,
                endpoint,
                campaignData,
                linkedCts.Token,
                headers);
            
            // Notify progress if callback provided
            options.NotifyProgress("MMS sent successfully");
            
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Notify progress if callback provided
            options.NotifyProgress("MMS sending cancelled");
            throw;
        }
        catch (OperationCanceledException)
        {
            // Notify progress if callback provided
            options.NotifyProgress("MMS sending timed out");
            throw new TimeoutException("MMS sending timed out");
        }
        catch (Exception ex)
        {
            // Notify progress if callback provided
            options.NotifyProgress("MMS sending failed");
            
            throw new InvalidOperationException($"Failed to send MMS: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Send a single MMS message to one recipient
    /// </summary>
    /// <param name="pictureFileKey">S3 file key for the image</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="phone">Recipient's phone number (E.164 format)</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the MMS send operation</param>
    /// <param name="forceNewCampaign">Whether to force a new campaign (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    public Task<SMSResponse> SendSingleAsync(
        string pictureFileKey,
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        string? customData = null,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true,
        CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            CustomData = customData
        };
        
        return SendAsync(
            pictureFileKey,
            new[] { account },
            message,
            title,
            senderPhone,
            options,
            forceNewCampaign,
            cancellationToken);
    }
    
    /// <summary>
    /// Complete MMS workflow: get signed URL, upload image, and send MMS
    /// </summary>
    /// <param name="imagePath">Path to the image file</param>
    /// <param name="contentType">MIME type of the image</param>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the MMS send operation</param>
    /// <param name="forceNewCampaign">Whether to force a new campaign (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    /// <exception cref="ArgumentException">If required parameters are missing or invalid</exception>
    /// <exception cref="InvalidOperationException">If any step of the process fails</exception>
    public async Task<SMSResponse> SendWithImageAsync(
        string imagePath,
        string contentType,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true,
        CancellationToken cancellationToken = default)
    {
        // Create options if not provided
        options ??= new SMSOptions();

        // Step 1: Compute MD5 and build a deterministic file key
        var md5Hash = ComputeMD5(imagePath);
        var extension = Path.GetExtension(imagePath).TrimStart('.');
        var fileKey = $"{_client.GetClientId()}/campaign/{md5Hash}.{extension}";

        // Step 2: Check if this file was already uploaded (MD5 cache)
        options.NotifyProgress("Checking if image already uploaded");
        var stored = await CheckFileUploadedAsync(fileKey, cancellationToken);

        if (string.IsNullOrEmpty(stored.StoredUrl))
        {
            // Step 3: Get a signed URL for uploading
            options.NotifyProgress("Getting signed upload URL");
            var fileName = $"{md5Hash}.{extension}";
            var uploadResponse = await GetSignedUploadUrlAsync(
                fileName,
                contentType,
                cancellationToken: cancellationToken);

            // Step 4: Upload the image to the signed URL
            options.NotifyProgress("Uploading image to S3");
            var uploadSuccess = await UploadImageToSignedUrlAsync(
                uploadResponse.SignedS3Url,
                imagePath,
                contentType,
                cancellationToken);

            if (!uploadSuccess)
            {
                throw new InvalidOperationException("Failed to upload image to S3");
            }

            options.NotifyProgress("Image uploaded successfully, sending MMS");
        }
        else
        {
            options.NotifyProgress("Image already uploaded (cache hit), sending MMS");
        }

        // Step 5: Send the MMS with the uploaded image
        return await SendAsync(
            fileKey,
            accounts,
            message,
            title,
            senderPhone,
            options,
            forceNewCampaign,
            cancellationToken);
    }
    
    /// <summary>
    /// Send an MMS message to one or more recipients (synchronous version)
    /// </summary>
    /// <param name="pictureFileKey">S3 file key for the image</param>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the MMS send operation</param>
    /// <param name="forceNewCampaign">Whether to force a new campaign (default: true)</param>
    /// <returns>API response</returns>
    public SMSResponse Send(
        string pictureFileKey,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true)
    {
        return SendAsync(pictureFileKey, accounts, message, title, senderPhone, options, forceNewCampaign)
            .GetAwaiter()
            .GetResult();
    }
    
    /// <summary>
    /// Send a single MMS message to one recipient (synchronous version)
    /// </summary>
    /// <param name="pictureFileKey">S3 file key for the image</param>
    /// <param name="firstName">Recipient's first name</param>
    /// <param name="lastName">Recipient's last name</param>
    /// <param name="phone">Recipient's phone number (E.164 format)</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the MMS send operation</param>
    /// <param name="forceNewCampaign">Whether to force a new campaign (default: true)</param>
    /// <returns>API response</returns>
    public SMSResponse SendSingle(
        string pictureFileKey,
        string firstName,
        string lastName,
        string phone,
        string message,
        string title,
        string? customData = null,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true)
    {
        return SendSingleAsync(pictureFileKey, firstName, lastName, phone, message, title, customData, senderPhone, options, forceNewCampaign)
            .GetAwaiter()
            .GetResult();
    }
    
    /// <summary>
    /// Complete MMS workflow: get signed URL, upload image, and send MMS (synchronous version)
    /// </summary>
    /// <param name="imagePath">Path to the image file</param>
    /// <param name="contentType">MIME type of the image</param>
    /// <param name="accounts">List of recipient accounts</param>
    /// <param name="message">Message content (can include ${FirstName} and ${LastName} variables)</param>
    /// <param name="title">Campaign title</param>
    /// <param name="options">Optional settings for the MMS send operation</param>
    /// <param name="forceNewCampaign">Whether to force a new campaign (default: true)</param>
    /// <returns>API response</returns>
    public SMSResponse SendWithImage(
        string imagePath,
        string contentType,
        IEnumerable<Account> accounts,
        string message,
        string title,
        string? senderPhone = null,
        SMSOptions? options = null,
        bool forceNewCampaign = true)
    {
        return SendWithImageAsync(imagePath, contentType, accounts, message, title, senderPhone, options, forceNewCampaign)
            .GetAwaiter()
            .GetResult();
    }
}
