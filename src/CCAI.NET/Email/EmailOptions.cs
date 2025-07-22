// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.Email;

/// <summary>
/// Options for email operations
/// </summary>
public record EmailOptions
{
    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int? Timeout { get; init; }
    
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int? Retries { get; init; }
    
    /// <summary>
    /// Callback for tracking progress
    /// </summary>
    public Action<string>? OnProgress { get; init; }
    
    /// <summary>
    /// Notify progress if callback is provided
    /// </summary>
    /// <param name="status">Progress status</param>
    public void NotifyProgress(string status)
    {
        OnProgress?.Invoke(status);
    }
}
