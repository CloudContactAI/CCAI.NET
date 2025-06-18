// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// Response from the signed URL API
/// </summary>
public class SignedUrlResponse
{
    /// <summary>
    /// The signed S3 URL for uploading the file
    /// </summary>
    [JsonPropertyName("signedS3Url")]
    public string SignedS3Url { get; set; } = string.Empty;
    
    /// <summary>
    /// The file key in S3
    /// </summary>
    [JsonPropertyName("fileKey")]
    public string FileKey { get; set; } = string.Empty;
    
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
