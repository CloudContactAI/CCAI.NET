# CCAI.NET v1.4.0 Release Notes - [CloudContactAI](https://cloudcontactai.com)

## 🚀 New Features

### Enhanced Webhook Support
- **New CloudContact Webhook Format**: Added support for the new CloudContact webhook event structure with 5 event types:
  - `message.sent` - Message successfully sent to recipient
  - `message.incoming` - Incoming message from recipient
  - `message.excluded` - Message excluded during campaign creation
  - `message.error.carrier` - Carrier-level delivery errors
  - `message.error.cloudcontact` - CloudContact system errors

- **New Webhook Classes**:
  - `CloudContactWebhookEvent` - Main webhook event wrapper
  - `WebhookEventData` - Comprehensive event data with pricing, segments, and error details
  - Enhanced `WebhookService` with `ParseCloudContactEvent()` method

### Updated Event Types
- **Breaking Change**: Renamed `MessageReceived` to `MessageIncoming` for consistency
- **Legacy Support**: Maintained backward compatibility with existing webhook implementations
- **New Event Classes**: Added `MessageIncomingEvent` to replace deprecated `MessageReceivedEvent`

## 🔧 Improvements

### Environment Variable Support
- **BasicExample**: Now loads `ClientId`, `ApiKey`, `FirstName`, `LastName`, and `Phone` from environment variables
- **Flexible Configuration**: Falls back to sensible defaults when environment variables are not set

### Code Quality
- **Cleaned Examples**: Removed 18+ redundant example files for better maintainability
- **Fixed Build Warnings**: Resolved all unreachable code and entry point conflicts
- **Improved Structure**: Streamlined examples directory with 10 core, comprehensive examples

## 📝 Enhanced Examples

### New Webhook Examples
- **webhook_cloudcontact_example.cs**: Demonstrates parsing new CloudContact webhook format
- **webhook_endpoint_example.cs**: Production-ready ASP.NET Core webhook controller
- **Enhanced webhook server**: Updated with emoji indicators and event-specific processing

### Updated Examples
- **BasicExample.cs**: Environment variable integration for account data
- **WebhookExample.cs**: Updated to use new `MessageIncoming` event type
- All webhook registration examples updated with new event types

## 🛠️ Technical Changes

### Webhook Service Enhancements
```csharp
// New method for parsing CloudContact webhooks
public CloudContactWebhookEvent? ParseCloudContactEvent(string json)

// Enhanced event data with new properties
public record WebhookEventData
{
    public decimal? TotalPrice { get; init; }
    public int? Segments { get; init; }
    public string? ExcludedReason { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    // ... and more
}
```

### Environment Variables
- `CCAI_CLIENT_ID` - Your CloudContact client ID
- `CCAI_API_KEY` - Your CloudContact API key
- `CCAI_FIRST_NAME` - Test recipient first name (optional)
- `CCAI_LAST_NAME` - Test recipient last name (optional)
- `CCAI_PHONE` - Test recipient phone number (optional)

## 🔄 Migration Guide

### Webhook Event Types
```csharp
// Old (deprecated)
WebhookEventType.MessageReceived

// New
WebhookEventType.MessageIncoming
```

### Webhook Event Classes
```csharp
// Old (deprecated)
MessageReceivedEvent receivedEvent

// New
MessageIncomingEvent incomingEvent
```

## 📦 Dependencies
- No new dependencies added
- Maintained compatibility with .NET 8.0
- DotNetEnv package already included for environment variable support

## 🐛 Bug Fixes
- Fixed build warnings related to unreachable code
- Resolved entry point conflicts in examples project
- Fixed webhook event type references in all examples

---

**Full Changelog**: Compare changes from v1.3.0 to v1.4.0 for complete details.
