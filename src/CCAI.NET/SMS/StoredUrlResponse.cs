// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace CCAI.NET.SMS;

/// <summary>
/// Response from the stored URL check API
/// </summary>
public class StoredUrlResponse
{
    /// <summary>
    /// The stored S3 URL of the uploaded file, empty if not found
    /// </summary>
    [JsonPropertyName("storedUrl")]
    public string StoredUrl { get; set; } = string.Empty;
}
