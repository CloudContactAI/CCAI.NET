// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// Custom converter for ID that can be either string or number
/// </summary>
public class FlexibleIdConverter : JsonConverter<string?>
{
    /// <summary>
    /// Reads and converts the JSON to a string value
    /// </summary>
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetInt64().ToString(),
            JsonTokenType.Null => null,
            _ => reader.GetString()
        };
    }

    /// <summary>
    /// Writes the string value to JSON
    /// </summary>
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

/// <summary>
/// Response from the SMS API
/// </summary>
public class SMSResponse
{
    /// <summary>
    /// Message ID
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleIdConverter))]
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
