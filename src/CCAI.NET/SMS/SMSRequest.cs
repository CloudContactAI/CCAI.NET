// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.SMS;

/// <summary>
/// Request model for SMS operations
/// </summary>
public record SMSRequest
{
    /// <summary>
    /// List of recipient accounts
    /// </summary>
    public required IEnumerable<Account> Accounts { get; init; }
    
    /// <summary>
    /// Message content (can include ${FirstName} and ${LastName} variables)
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Campaign title
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// Custom data to be included with the message
    /// </summary>
    public string? CustomData { get; init; }
    
    /// <summary>
    /// Optional settings for the SMS send operation
    /// </summary>
    public SMSOptions? Options { get; init; }
    
    /// <summary>
    /// Optional sender phone number
    /// </summary>
    public string? SenderPhone { get; init; }
    
    /// <summary>
    /// Create an SMS request for multiple recipients
    /// </summary>
    public static SMSRequest Create(IEnumerable<Account> accounts, string message, string title, string? customData = null, SMSOptions? options = null, string? senderPhone = null)
    {
        return new SMSRequest
        {
            Accounts = accounts,
            Message = message,
            Title = title,
            CustomData = customData,
            Options = options,
            SenderPhone = senderPhone
        };
    }
    
    /// <summary>
    /// Create an SMS request for a single recipient
    /// </summary>
    public static SMSRequest CreateSingle(string firstName, string lastName, string phone, string message, string title, string? customAccountId = null, string? customData = null, SMSOptions? options = null, string? senderPhone = null)
    {
        var account = new Account
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            CustomAccountId = customAccountId,
            CustomData = customData
        };
        
        return new SMSRequest
        {
            Accounts = new[] { account },
            Message = message,
            Title = title,
            CustomData = customData,
            Options = options,
            SenderPhone = senderPhone
        };
    }
}