# CloudContactAI.CCAI.NET v1.3.0 Release Notes

**Release Date**: August 26, 2025  
**Package**: CloudContactAI.CCAI.NET v1.3.0  
**Compatibility**: Fully backward compatible with v1.2.0, v1.1.0, and v1.0.0

## 🎉 What's New in v1.3.0

### 🔧 Enhanced SMS Custom Data Support
Major improvements to SMS functionality with comprehensive custom data support for webhook integration:

- **🏷️ Custom Data Parameters**: Full support for custom data fields in SMS messages for enhanced webhook tracking
- **🔗 Webhook Integration**: Seamless integration with webhook callbacks including custom data in payloads
- **📊 Enhanced Testing Framework**: Comprehensive testing suite for custom data webhook functionality
- **🧪 Live Testing Examples**: Working examples for testing custom data webhook integration

### 🌐 Environment Variable Configuration
Flexible configuration system supporting both production and test environments:

- **⚙️ Environment-Based URLs**: Support for configurable API endpoints via environment variables
- **🔄 Test Environment Support**: Seamless switching between production and test environments
- **🛡️ Secure Configuration**: Environment variable support for sensitive configuration data
- **📋 Configuration Templates**: Sample environment files for easy setup

## 🚀 New Features

### Custom Data SMS Functionality
```csharp
// Send SMS with custom data for webhook tracking
var response = await ccai.SMS.SendSingleAsync(
    firstName: "John",
    lastName: "Doe",
    phone: "+1234567890",
    message: "Hello ${FirstName}! Your order #12345 is ready.",
    title: "Order Notification",
    customData: "OrderID:12345,CustomerType:Premium,Source:WebApp"
);

// Custom data will be included in webhook callbacks
Console.WriteLine($"Message sent with custom data: {response.Id}");
```

### Environment Variable Configuration
```csharp
// Configure using environment variables
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY"),
    UseTestEnvironment = true  // Automatically uses test URLs from env vars
};

// URLs are automatically resolved from environment variables:
// CCAI_BASE_URL / CCAI_TEST_BASE_URL
// CCAI_EMAIL_BASE_URL / CCAI_TEST_EMAIL_BASE_URL  
// CCAI_AUTH_BASE_URL / CCAI_TEST_AUTH_BASE_URL
```

### Enhanced Webhook Testing
```csharp
// Complete webhook testing with custom data
var customData = $"TestData_{Guid.NewGuid().ToString()[..8]}";
var response = await ccai.SMS.SendSingleAsync(
    firstName: "Test",
    lastName: "User", 
    phone: "+1234567890",
    message: "Webhook test message with tracking data",
    title: "Webhook Test Campaign",
    customData: customData
);

// Webhook payload will include:
// - CustomData field with your tracking information
// - Message delivery status updates
// - Inbound reply message notifications
```

## 🔧 Technical Improvements

### SMS Service Enhancements
- **Custom Data Support**: Native support for custom data parameters in all SMS methods
- **Webhook Integration**: Enhanced webhook payload structure with custom data fields
- **Request/Response Models**: Updated SMS request and response models with custom data properties
- **Validation**: Improved parameter validation and error handling

### Configuration System
- **Environment Variables**: Automatic resolution of API endpoints from environment variables
- **Test Environment**: Seamless switching between production and test environments
- **URL Flexibility**: Support for custom API endpoints while maintaining defaults
- **Backward Compatibility**: Existing hardcoded configurations continue to work

### Testing Framework
- **Custom Data Tests**: Comprehensive test suite for custom data functionality
- **Webhook Testing**: Live webhook testing examples with real payload validation
- **Environment Testing**: Tests for both production and test environment configurations
- **Integration Tests**: End-to-end testing of SMS with webhook callbacks

## 📦 New Examples and Documentation

### SMS Custom Data Examples
- **`sms-custom-data-test/`**: Complete example project for testing custom data functionality
- **`sms_custom_data_webhook_test.cs`**: Standalone example for webhook testing with custom data
- **Live Testing Guide**: Step-by-step instructions for testing webhook integration

### Environment Configuration
- **`.env.sample`**: Updated sample environment file with all supported variables
- **Configuration Examples**: Multiple examples showing different configuration approaches
- **Test Environment Setup**: Complete guide for setting up test environment

### Webhook Integration
- **Enhanced Webhook Examples**: Updated webhook examples with custom data handling
- **Payload Documentation**: Complete documentation of webhook payload structure
- **Testing Workflows**: End-to-end testing workflows for webhook integration

## 🔄 API Changes and Enhancements

### SMS Service Methods
```csharp
// Enhanced SendSingleAsync with custom data support
public async Task<SMSResponse> SendSingleAsync(
    string firstName,
    string lastName, 
    string phone,
    string message,
    string title,
    string? customData = null,  // NEW: Custom data parameter
    SMSOptions? options = null,
    CancellationToken cancellationToken = default)

// Enhanced SendAsync with custom data support  
public async Task<SMSResponse> SendAsync(
    SMSRequest request,  // SMSRequest now includes CustomData property
    CancellationToken cancellationToken = default)
```

### Configuration Enhancements
```csharp
public record CCAIConfig
{
    // Environment variable support with fallback defaults
    public string BaseUrl { get; init; } = 
        Environment.GetEnvironmentVariable("CCAI_BASE_URL") ?? 
        "https://core.cloudcontactai.com/api";
        
    public string EmailBaseUrl { get; init; } = 
        Environment.GetEnvironmentVariable("CCAI_EMAIL_BASE_URL") ?? 
        "https://email-campaigns.cloudcontactai.com";
        
    public string AuthBaseUrl { get; init; } = 
        Environment.GetEnvironmentVariable("CCAI_AUTH_BASE_URL") ?? 
        "https://auth.cloudcontactai.com";
        
    // Test environment support
    public bool UseTestEnvironment { get; init; } = false;
    
    // Dynamic URL resolution based on environment
    public string GetBaseUrl() { /* ... */ }
    public string GetEmailBaseUrl() { /* ... */ }
    public string GetAuthBaseUrl() { /* ... */ }
}
```

### Request/Response Models
```csharp
// Enhanced SMS request with custom data
public class SMSRequest
{
    public IEnumerable<Account> Accounts { get; set; }
    public string Message { get; set; }
    public string Title { get; set; }
    public string? CustomData { get; set; }  // NEW: Custom data field
    public SMSOptions? Options { get; set; }
}

// Enhanced SMS response with additional metadata
public class SMSResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public string? CampaignId { get; set; }
    public int? MessagesSent { get; set; }
    public string? Timestamp { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}
```

## 🌍 Environment Variable Reference

### Production Environment
```bash
# Production API endpoints
CCAI_BASE_URL=https://core.cloudcontactai.com/api
CCAI_EMAIL_BASE_URL=https://email-campaigns.cloudcontactai.com  
CCAI_AUTH_BASE_URL=https://auth.cloudcontactai.com
```

### Test Environment  
```bash
# Test API endpoints
CCAI_TEST_BASE_URL=https://core-test-cloudcontactai.allcode.com/api
CCAI_TEST_EMAIL_BASE_URL=https://email-campaigns-test-cloudcontactai.allcode.com
CCAI_TEST_AUTH_BASE_URL=https://auth-test-cloudcontactai.allcode.com
```

### Authentication
```bash
# API credentials
CCAI_CLIENT_ID=your-client-id
CCAI_API_KEY=your-api-key
```

## 🧪 Testing and Validation

### Custom Data Webhook Testing
1. **Setup Environment**: Configure webhook URL in CCAI dashboard
2. **Send Test SMS**: Use custom data examples to send test messages
3. **Monitor Webhooks**: Verify custom data appears in webhook payloads
4. **Validate Responses**: Confirm delivery status and reply handling

### Environment Configuration Testing
1. **Production Testing**: Verify production environment connectivity
2. **Test Environment**: Validate test environment functionality  
3. **URL Override**: Test custom URL configuration
4. **Fallback Behavior**: Verify default URL fallback when env vars not set

## 🔄 Migration Guide

### From v1.2.0 to v1.3.0
**No breaking changes!** Simply update your package reference:

```bash
dotnet add package CloudContactAI.CCAI.NET --version 1.3.0
```

### Leveraging New Features
```csharp
// Add custom data to existing SMS calls
var response = await smsService.SendSingleAsync(
    firstName: "John",
    lastName: "Doe", 
    phone: "+1234567890",
    message: "Your message",
    title: "Campaign Title",
    customData: "TrackingID:12345"  // NEW: Add this parameter
);

// Use environment variables for configuration
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY"),
    UseTestEnvironment = true  // NEW: Enable test environment
};
```

## 📊 Performance and Reliability

### Enhanced Error Handling
- **Validation**: Improved parameter validation with descriptive error messages
- **Retry Logic**: Enhanced retry mechanisms for failed requests
- **Timeout Handling**: Better timeout management for long-running operations
- **Connection Management**: Optimized HTTP client usage and connection pooling

### Webhook Reliability
- **Payload Validation**: Comprehensive validation of webhook payloads
- **Custom Data Integrity**: Ensures custom data is properly transmitted and received
- **Delivery Tracking**: Enhanced delivery status tracking and reporting
- **Error Recovery**: Improved error handling for webhook delivery failures

## 🐛 Bug Fixes

- **Phone Number Validation**: Fixed phone number format validation in examples
- **Environment Loading**: Improved environment variable loading and error handling
- **Custom Data Encoding**: Fixed custom data encoding issues in webhook payloads
- **Test Environment URLs**: Corrected test environment URL resolution logic
- **Example Projects**: Fixed compilation issues in example projects

## 📞 Support and Resources

### Documentation
- **API Reference**: Complete XML documentation for all new methods
- **Examples**: Working examples for all new functionality
- **Webhook Guide**: Comprehensive webhook integration guide
- **Environment Setup**: Step-by-step environment configuration guide

### Testing Resources
- **Test Projects**: Complete test projects for validation
- **Sample Data**: Sample custom data formats and examples
- **Webhook Payloads**: Example webhook payload structures
- **Troubleshooting**: Common issues and solutions guide

## 🎯 What's Next

Future releases may include:
- **Advanced Custom Data**: Structured custom data with schema validation
- **Webhook Filtering**: Advanced webhook filtering based on custom data
- **Batch Operations**: Enhanced batch processing with custom data support
- **Analytics Integration**: Built-in analytics and reporting for custom data
- **Template Engine**: Advanced message templating with custom data integration

---

**CloudContactAI.CCAI.NET v1.3.0 - Enhanced SMS functionality with comprehensive custom data support and flexible environment configuration! 🚀📱**

*This version significantly enhances SMS capabilities with custom data support for advanced webhook integration while maintaining full backward compatibility and adding flexible environment-based configuration.*
