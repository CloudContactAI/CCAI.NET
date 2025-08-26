// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.Email;

/// <summary>
/// Request model for Email operations
/// </summary>
public record EmailRequest
{
    /// <summary>
    /// List of recipient accounts
    /// </summary>
    public required IList<EmailAccount> Accounts { get; init; }
    
    /// <summary>
    /// Email subject
    /// </summary>
    public required string Subject { get; init; }
    
    /// <summary>
    /// Campaign title
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// HTML message content
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Sender's email address
    /// </summary>
    public required string SenderEmail { get; init; }
    
    /// <summary>
    /// Reply-to email address
    /// </summary>
    public required string ReplyEmail { get; init; }
    
    /// <summary>
    /// Sender's name
    /// </summary>
    public required string SenderName { get; init; }
    
    /// <summary>
    /// Optional settings for the email send operation
    /// </summary>
    public EmailOptions? Options { get; init; }
    
    /// <summary>
    /// Create an email request for multiple recipients
    /// </summary>
    public static EmailRequest Create(IList<EmailAccount> accounts, string subject, string title, string message, string senderEmail, string replyEmail, string senderName, EmailOptions? options = null)
    {
        return new EmailRequest
        {
            Accounts = accounts,
            Subject = subject,
            Title = title,
            Message = message,
            SenderEmail = senderEmail,
            ReplyEmail = replyEmail,
            SenderName = senderName,
            Options = options
        };
    }
    
    /// <summary>
    /// Create an email request for a single recipient
    /// </summary>
    public static EmailRequest CreateSingle(string firstName, string lastName, string email, string subject, string title, string message, string senderEmail, string replyEmail, string senderName, string? customAccountId = null, EmailOptions? options = null)
    {
        var account = new EmailAccount
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CustomAccountId = customAccountId
        };
        
        return new EmailRequest
        {
            Accounts = new List<EmailAccount> { account },
            Subject = subject,
            Title = title,
            Message = message,
            SenderEmail = senderEmail,
            ReplyEmail = replyEmail,
            SenderName = senderName,
            Options = options
        };
    }
}