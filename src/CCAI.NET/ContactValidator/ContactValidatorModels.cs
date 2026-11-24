// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.ContactValidator;

/// <summary>
/// The six checks of an email validation, each with its own Amazon SES verdict:
/// "HIGH", "MEDIUM", "LOW" or "UNKNOWN" (a verdict SES did not return).
/// </summary>
public class EmailValidationChecks
{
    /// <summary>The address follows RFC format rules (good value: HIGH)</summary>
    public string? HasValidSyntax { get; set; }

    /// <summary>The domain exists and has DNS/MX records to receive mail (good value: HIGH)</summary>
    public string? HasValidDnsRecords { get; set; }

    /// <summary>The mailbox exists and can receive messages (good value: HIGH)</summary>
    public string? MailboxExists { get; set; }

    /// <summary>A role address such as admin@, support@ or info@ (good value: LOW)</summary>
    public string? IsRoleAddress { get; set; }

    /// <summary>A disposable or temporary email provider (good value: LOW)</summary>
    public string? IsDisposable { get; set; }

    /// <summary>The address looks randomly generated (good value: LOW)</summary>
    public string? IsRandomInput { get; set; }
}

/// <summary>
/// Validation result for a single email address
/// </summary>
public class EmailValidationResult
{
    /// <summary>
    /// The validated email address
    /// </summary>
    [JsonPropertyName("contactField")]
    public string ContactField { get; set; } = string.Empty;

    /// <summary>
    /// Contact type — always "email"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Validation status: "valid", "invalid", or "risky"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata (e.g. safe_to_send, ai_verdict)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, JsonElement>? Metadata { get; set; }

    // The properties below are read from Metadata. They are null when the response does not have them: the account
    // does not have Email Address Validation, or the address was not checked (see Metadata["error"]).

    /// <summary>
    /// The overall verdict: "HIGH", "MEDIUM", "LOW" or "UNKNOWN"
    /// </summary>
    [JsonIgnore]
    public string? Verdict => MetadataString("verdict");

    /// <summary>
    /// The six checks, each with its own verdict
    /// </summary>
    [JsonIgnore]
    public EmailValidationChecks? Checks
    {
        get
        {
            if (Metadata == null || !Metadata.TryGetValue("checks", out var checks) || checks.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            string? Verdict(string name) =>
                checks.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

            return new EmailValidationChecks
            {
                HasValidSyntax = Verdict("hasValidSyntax"),
                HasValidDnsRecords = Verdict("hasValidDnsRecords"),
                MailboxExists = Verdict("mailboxExists"),
                IsRoleAddress = Verdict("isRoleAddress"),
                IsDisposable = Verdict("isDisposable"),
                IsRandomInput = Verdict("isRandomInput")
            };
        }
    }

    /// <summary>
    /// When Amazon SES checked the address. Null when the address was rejected without a check.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset? ValidatedAt =>
        Metadata != null && Metadata.TryGetValue("validatedAt", out var value)
            && value.ValueKind == JsonValueKind.String && value.TryGetDateTimeOffset(out var validatedAt)
            ? validatedAt
            : null;

    /// <summary>
    /// True when the result is a saved check from the last 30 days, instead of a new one
    /// </summary>
    [JsonIgnore]
    public bool? Reused =>
        Metadata != null && Metadata.TryGetValue("reused", out var value)
            && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            ? value.GetBoolean()
            : null;

    private string? MetadataString(string name) =>
        Metadata != null && Metadata.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}

/// <summary>
/// Validation result for a single phone number
/// </summary>
public class PhoneValidationResult
{
    /// <summary>
    /// The validated phone number
    /// </summary>
    [JsonPropertyName("contactField")]
    public string ContactField { get; set; } = string.Empty;

    /// <summary>
    /// Contact type — always "phone"
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Validation status: "valid", "invalid", or "landline"
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata (e.g. country_code, national_number, carrier_type)
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, JsonElement>? Metadata { get; set; }
}

/// <summary>
/// Aggregate counts for a bulk validation response
/// </summary>
public class ValidationSummary
{
    /// <summary>Total contacts validated</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Number of valid contacts</summary>
    [JsonPropertyName("valid")]
    public int Valid { get; set; }

    /// <summary>Number of invalid contacts</summary>
    [JsonPropertyName("invalid")]
    public int Invalid { get; set; }

    /// <summary>Number of risky contacts (emails only)</summary>
    [JsonPropertyName("risky")]
    public int Risky { get; set; }

    /// <summary>Number of landline numbers (phones only)</summary>
    [JsonPropertyName("landline")]
    public int Landline { get; set; }
}

/// <summary>
/// Response for a bulk email validation request
/// </summary>
public class BulkEmailValidationResult
{
    /// <summary>Individual validation results</summary>
    [JsonPropertyName("results")]
    public List<EmailValidationResult> Results { get; set; } = new();

    /// <summary>Aggregate summary</summary>
    [JsonPropertyName("summary")]
    public ValidationSummary Summary { get; set; } = new();
}

/// <summary>
/// Response for a bulk phone validation request
/// </summary>
public class BulkPhoneValidationResult
{
    /// <summary>Individual validation results</summary>
    [JsonPropertyName("results")]
    public List<PhoneValidationResult> Results { get; set; } = new();

    /// <summary>Aggregate summary</summary>
    [JsonPropertyName("summary")]
    public ValidationSummary Summary { get; set; } = new();
}

/// <summary>
/// Phone number input for bulk validation
/// </summary>
public class PhoneInput
{
    /// <summary>
    /// Phone number in E.164 format (e.g. +15551234567)
    /// </summary>
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Optional ISO 3166-1 alpha-2 country code (e.g. "US")
    /// </summary>
    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}
