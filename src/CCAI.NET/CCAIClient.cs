// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CCAI.NET.Email;
using CCAI.NET.SMS;
using CCAI.NET.Webhook;

namespace CCAI.NET;

/// <summary>
/// Configuration for the CCAI client
/// </summary>
public record CCAIConfig
{
    /// <summary>
    /// Client ID for authentication
    /// </summary>
    public required string ClientId { get; init; }
    
    /// <summary>
    /// API key for authentication
    /// </summary>
    public required string ApiKey { get; init; }
    
    /// <summary>
    /// Base URL for the SMS/MMS API
    /// </summary>
    public string BaseUrl { get; init; } = "https://core.cloudcontactai.com/api";
    
    /// <summary>
    /// Base URL for the Email API
    /// </summary>
    public string EmailBaseUrl { get; init; } = "https://email-campaigns.cloudcontactai.com";
    
    /// <summary>
    /// Base URL for the Auth API
    /// </summary>
    public string AuthBaseUrl { get; init; } = "https://auth.cloudcontactai.com";
    
    /// <summary>
    /// Whether to use test environment URLs
    /// </summary>
    public bool UseTestEnvironment { get; init; } = false;
    
    /// <summary>
    /// Get the appropriate base URL based on environment
    /// </summary>
    public string GetBaseUrl()
    {
        return UseTestEnvironment 
            ? "https://core-test-cloudcontactai.allcode.com/api" 
            : BaseUrl;
    }
    
    /// <summary>
    /// Get the appropriate email base URL based on environment
    /// </summary>
    public string GetEmailBaseUrl()
    {
        return UseTestEnvironment 
            ? "https://email-campaigns-test-cloudcontactai.allcode.com" 
            : EmailBaseUrl;
    }
    
    /// <summary>
    /// Get the appropriate auth base URL based on environment
    /// </summary>
    public string GetAuthBaseUrl()
    {
        return UseTestEnvironment 
            ? "https://auth-test-cloudcontactai.allcode.com" 
            : AuthBaseUrl;
    }
}

/// <summary>
/// Main client for interacting with the CloudContactAI API
/// </summary>
public class CCAIClient : IDisposable
{
    private readonly CCAIConfig _config;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    /// <summary>
    /// SMS service for sending messages
    /// </summary>
    public SMSService SMS { get; }
    
    /// <summary>
    /// MMS service for sending multimedia messages
    /// </summary>
    public MMSService MMS { get; }
    
    /// <summary>
    /// Email service for sending email campaigns
    /// </summary>
    public EmailService Email { get; }
    
    /// <summary>
    /// Webhook service for managing webhooks
    /// </summary>
    public WebhookService Webhook { get; }

    /// <summary>
    /// Create a new CCAI client instance
    /// </summary>
    /// <param name="config">Configuration for the client</param>
    /// <param name="httpClient">Optional HTTP client</param>
    /// <exception cref="ArgumentNullException">If required configuration is missing</exception>
    public CCAIClient(CCAIConfig config, HttpClient? httpClient = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        
        if (string.IsNullOrEmpty(config.ClientId))
        {
            throw new ArgumentException("Client ID is required", nameof(config.ClientId));
        }
        
        if (string.IsNullOrEmpty(config.ApiKey))
        {
            throw new ArgumentException("API Key is required", nameof(config.ApiKey));
        }
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
        
        if (httpClient == null)
        {
            _httpClient = new HttpClient();
            _disposeHttpClient = true;
        }
        else
        {
            _httpClient = httpClient;
            _disposeHttpClient = false;
        }
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        SMS = new SMSService(this);
        MMS = new MMSService(this);
        Email = new EmailService(this);
        Webhook = new WebhookService(this);
    }

    /// <summary>
    /// Get the client ID
    /// </summary>
    /// <returns>Client ID</returns>
    public string GetClientId() => _config.ClientId;

    /// <summary>
    /// Get the API key
    /// </summary>
    /// <returns>API key</returns>
    public string GetApiKey() => _config.ApiKey;

    /// <summary>
    /// Get the base URL
    /// </summary>
    /// <returns>Base URL</returns>
    public string GetBaseUrl() => _config.GetBaseUrl();
    
    /// <summary>
    /// Get the email base URL
    /// </summary>
    /// <returns>Email base URL</returns>
    public string GetEmailBaseUrl() => _config.GetEmailBaseUrl();
    
    /// <summary>
    /// Get the auth base URL
    /// </summary>
    /// <returns>Auth base URL</returns>
    public string GetAuthBaseUrl() => _config.GetAuthBaseUrl();

    /// <summary>
    /// Make an authenticated API request to the CCAI API
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="headers">Additional headers</param>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <returns>API response</returns>
    /// <exception cref="HttpRequestException">If the API returns an error</exception>
    public async Task<TResponse> RequestAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default,
        Dictionary<string, string>? headers = null)
    {
        return await CustomRequestAsync<TResponse>(method, endpoint, data, _config.GetBaseUrl(), cancellationToken, headers);
    }

    /// <summary>
    /// Make an authenticated API request to a custom API endpoint
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="baseUrl">Custom base URL for the API</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="headers">Additional headers</param>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <returns>API response</returns>
    /// <exception cref="HttpRequestException">If the API returns an error</exception>
    public async Task<TResponse> CustomRequestAsync<TResponse>(
        HttpMethod method,
        string endpoint,
        object? data = null,
        string? baseUrl = null,
        CancellationToken cancellationToken = default,
        Dictionary<string, string>? headers = null)
    {
        var url = $"{baseUrl ?? _config.GetBaseUrl()}{endpoint}";
        
        using var request = new HttpRequestMessage(method, url);
        
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        // Add additional headers if provided
        if (headers != null)
        {
            foreach (var (key, value) in headers)
            {
                request.Headers.Add(key, value);
            }
        }
        
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        
        // Throw an exception for HTTP errors
        response.EnsureSuccessStatusCode();
        
        // Parse the response as JSON
        var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
        
        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize response");
        }
        
        return result;
    }

    /// <summary>
    /// Make an authenticated API request to the CCAI API with a dynamic response
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="headers">Additional headers</param>
    /// <returns>API response as a dictionary</returns>
    /// <exception cref="HttpRequestException">If the API returns an error</exception>
    public async Task<Dictionary<string, JsonElement>> RequestAsync(
        HttpMethod method,
        string endpoint,
        object? data = null,
        CancellationToken cancellationToken = default,
        Dictionary<string, string>? headers = null)
    {
        return await RequestAsync<Dictionary<string, JsonElement>>(method, endpoint, data, cancellationToken, headers);
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}
