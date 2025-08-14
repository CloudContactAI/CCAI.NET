# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-08-14

### Added
- **Webhook Testing Infrastructure**: Complete webhook testing setup with ngrok integration
- **Comprehensive Examples**: Added multiple working examples in the `/examples` directory
  - SMS sending examples (`sms_send.cs`)
  - MMS sending examples (`mms_send.cs`) 
  - Email sending examples (`test_email_sender.cs`)
  - Webhook server examples (`webhook-server/Program.cs`)
  - Webhook registration examples (`webhook_register.cs`)
- **Local Development Support**: Enhanced local development experience
  - Fixed build issues with duplicate assembly attributes
  - Improved project structure and dependencies
  - Added comprehensive `.env` file support
- **Documentation Improvements**: 
  - Updated README.md with step-by-step webhook testing guide
  - Added ngrok installation and configuration instructions
  - Included real webhook payload examples
  - Added troubleshooting section

### Fixed
- **Build System**: Resolved duplicate assembly attribute compilation errors
- **Project Dependencies**: Fixed project references and package dependencies
- **Example Projects**: All example projects now build and run successfully
- **Environment Configuration**: Improved environment variable loading and validation

### Changed
- **Package Version**: Bumped from 1.0.0 to 1.1.0
- **Project Structure**: Cleaned up example projects and removed conflicting files
- **Documentation**: Enhanced README with comprehensive webhook testing workflow

### Technical Details
- **Target Framework**: .NET 8.0
- **Dependencies**: 
  - Microsoft.Extensions.Http 8.0.0
  - System.Net.Http.Json 8.0.0
  - System.Text.Json 8.0.5
  - DotNetEnv 3.1.1 (for examples)
- **Build Configuration**: Fixed assembly generation conflicts
- **Testing**: Added comprehensive package testing suite

### Webhook Testing Features
- **ngrok Integration**: Complete setup guide for local webhook testing
- **Bidirectional Testing**: Support for both outbound delivery and inbound response webhooks
- **Real-time Monitoring**: Live webhook event logging and debugging
- **CCAI Dashboard Integration**: Instructions for configuring webhooks in CCAI settings

### Developer Experience
- **Local Package Testing**: Added test project for validating package functionality
- **Environment Setup**: Streamlined `.env` configuration
- **Error Handling**: Improved error messages and debugging information
- **Examples**: Working examples for all major features

### Breaking Changes
None. This release is fully backward compatible with v1.0.0.

### Migration Guide
No migration required. Simply update your package reference:
```bash
dotnet add package CloudContactAI.CCAI.NET --version 1.1.0
```

---

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

### Dependencies
- .NET 8.0 target framework
- Microsoft.Extensions.Http
- System.Net.Http.Json
- System.Text.Json
