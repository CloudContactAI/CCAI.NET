# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.0] - 2025-09-03

### Added
- **Enhanced Webhook Support**: New CloudContact webhook event structure with 5 event types
  - `message.sent` - Message successfully sent to recipient
  - `message.incoming` - Incoming message from recipient  
  - `message.excluded` - Message excluded during campaign creation
  - `message.error.carrier` - Carrier-level delivery errors
  - `message.error.cloudcontact` - CloudContact system errors
- **New Webhook Classes**:
  - `CloudContactWebhookEvent` - Main webhook event wrapper
  - `WebhookEventData` - Comprehensive event data with pricing, segments, and error details
  - Enhanced `WebhookService` with `ParseCloudContactEvent()` method
- **Environment Variable Support**: 
  - `BasicExample` now loads configuration from environment variables
  - Support for `CCAI_CLIENT_ID`, `CCAI_API_KEY`, `CCAI_FIRST_NAME`, `CCAI_LAST_NAME`, `CCAI_PHONE`
- **Production-Ready Examples**:
  - `webhook_endpoint_example.cs` - ASP.NET Core webhook controller
  - `webhook_cloudcontact_example.cs` - CloudContact webhook format demonstration
  - Enhanced webhook server with emoji indicators and event-specific processing

### Changed
- **Breaking Change**: Renamed `MessageReceived` to `MessageIncoming` for consistency
- **Updated Event Classes**: `MessageIncomingEvent` replaces deprecated `MessageReceivedEvent`
- **Improved Examples**: Streamlined examples directory with 10 core, comprehensive examples
- **Enhanced Documentation**: Updated package description and release notes

### Fixed
- **Build Warnings**: Resolved all unreachable code and entry point conflicts
- **Code Quality**: Cleaned up duplicate and orphaned code
- **Project Structure**: Removed 18+ redundant example files for better maintainability

### Removed
- **Redundant Files**: Cleaned up examples directory and root directory
- **Old Release Notes**: Consolidated to single current version
- **Deprecated Classes**: Removed unused webhook event classes

### Migration Guide
```csharp
// Update webhook event types
WebhookEventType.MessageReceived → WebhookEventType.MessageIncoming

// Update event classes  
MessageReceivedEvent → MessageIncomingEvent
```

## [1.3.0] - 2025-08-26

### Added
- **Advanced Webhook Features**: Enhanced webhook processing capabilities
- **Custom Data Support**: Extended SMS sending with custom data fields
- **Progress Tracking**: Added progress tracking for campaign monitoring
- **Enhanced Error Handling**: Improved error reporting and debugging

### Changed
- **Package Metadata**: Updated package tags and description
- **Documentation**: Enhanced README with advanced usage examples

### Fixed
- **Webhook Processing**: Improved webhook event parsing and handling
- **Memory Management**: Optimized resource usage in HTTP client

## [1.2.0] - 2025-08-14

### Added
- **MMS Enhancements**: Improved MMS sending with better image handling
- **Email Templates**: Added email template support
- **Batch Processing**: Enhanced bulk message processing capabilities

### Changed
- **API Improvements**: Streamlined API method signatures
- **Performance**: Optimized HTTP request handling

### Fixed
- **Content-Type Handling**: Fixed MMS content type detection
- **Async Operations**: Improved async/await patterns

## [1.1.0] - 2025-08-14

### Added
- **Webhook Testing Infrastructure**: Complete webhook testing setup with ngrok integration
- **Comprehensive Examples**: Added multiple working examples in the `/examples` directory
- **Local Development Support**: Enhanced local development experience
- **Documentation Improvements**: Updated README with step-by-step webhook testing guide

### Fixed
- **Build System**: Resolved duplicate assembly attribute compilation errors
- **Project Dependencies**: Fixed project references and package dependencies
- **Example Projects**: All example projects now build and run successfully

### Changed
- **Package Version**: Bumped from 1.0.0 to 1.1.0
- **Project Structure**: Cleaned up example projects and removed conflicting files

## [1.0.0] - 2025-06-09

### Added
- Initial release of CloudContactAI.CCAI.NET
- SMS sending functionality
- MMS sending functionality  
- Email sending functionality
- Basic client configuration
- Account management models
- HTTP client integration
- JSON serialization support

### Features
- Send single SMS messages
- Send bulk SMS campaigns
- Send MMS with image attachments
- Send email campaigns
- Variable substitution in messages
- Async/await support
- Comprehensive error handling
