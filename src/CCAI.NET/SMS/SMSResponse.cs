using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// Response from the SMS API
/// </summary>
public class SMSResponse
{
    /// <summary>
    /// Message ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// Message status
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    /// <summary>
    /// Campaign ID
    /// </summary>
    [JsonPropertyName("campaignId")]
    public string? CampaignId { get; set; }
    
    /// <summary>
    /// Number of messages sent
    /// </summary>
    [JsonPropertyName("messagesSent")]
    public int? MessagesSent { get; set; }
    
    /// <summary>
    /// Timestamp of the operation
    /// </summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
    
    /// <summary>
    /// Additional data from the API
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    
    /// <summary>
    /// Get a value from the additional data
    /// </summary>
    /// <param name="key">Property name</param>
    /// <typeparam name="T">Property type</typeparam>
    /// <returns>Property value or default</returns>
    public T? GetValue<T>(string key)
    {
        if (AdditionalData == null || !AdditionalData.TryGetValue(key, out var element))
        {
            return default;
        }
        
        return element.Deserialize<T>();
    }
}
