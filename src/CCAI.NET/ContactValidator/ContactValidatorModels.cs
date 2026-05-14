// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCAI.NET.ContactValidator;

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
