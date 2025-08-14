# CloudContactAI.CCAI.NET v1.1.0 Release Notes

**Release Date**: August 14, 2025  
**Package**: CloudContactAI.CCAI.NET v1.1.0  
**Compatibility**: Fully backward compatible with v1.0.0

## 🎉 What's New in v1.1.0

### 🔗 Webhook Testing Infrastructure
Complete webhook testing setup that allows developers to test webhook functionality locally using ngrok:

- **ngrok Integration**: Step-by-step guide for setting up ngrok tunnels
- **Local Webhook Server**: Ready-to-use ASP.NET Core webhook server
- **Bidirectional Testing**: Test both outbound delivery notifications and inbound message responses
- **Real-time Monitoring**: Live webhook event logging with detailed headers and payloads

### 📚 Comprehensive Examples
Added a complete `/examples` directory with working code samples:

- **SMS Examples**: `sms_send.cs` - Send single SMS messages with variable substitution
- **MMS Examples**: `mms_send.cs` - Send MMS with image attachments
- **Email Examples**: `test_email_sender.cs` - Send email campaigns
- **Webhook Examples**: Complete webhook server and registration examples
- **Environment Configuration**: Proper `.env` file setup and usage

### 🛠️ Developer Experience Improvements

#### Build System Fixes
- ✅ Resolved duplicate assembly attribute compilation errors
- ✅ Fixed project dependencies and references
- ✅ All example projects now build and run successfully

#### Documentation Enhancements
- 📖 Updated README.md with comprehensive webhook testing guide
- 🔧 Added ngrok installation and configuration instructions
- 📋 Included real webhook payload examples from actual CCAI responses
- 🐛 Added troubleshooting section for common issues

#### Local Development Support
- 🏠 Enhanced local development experience with proper project structure
- ⚙️ Improved environment variable loading and validation
- 🧪 Added package testing suite for validation

## 🔧 Technical Improvements

### Package Structure
```
CloudContactAI.CCAI.NET v1.1.0
├── Core Library (unchanged - backward compatible)
├── Examples/ (new)
│   ├── sms_send.cs
│   ├── mms_send.cs
│   ├── test_email_sender.cs
│   ├── webhook-server/
│   └── webhook_register.cs
├── Documentation (enhanced)
│   ├── README.md (updated)
│   ├── CHANGELOG.md (new)
│   └── RELEASE-NOTES-v1.1.0.md (new)
└── Tests/ (new)
    └── Package validation suite
```

### Dependencies
- **Target Framework**: .NET 8.0
- **Core Dependencies** (unchanged):
  - Microsoft.Extensions.Http 8.0.0
  - System.Net.Http.Json 8.0.0
  - System.Text.Json 8.0.5
- **Example Dependencies**:
  - DotNetEnv 3.1.1
  - Microsoft.AspNetCore.App (for webhook server)

## 🚀 Getting Started with v1.1.0

### Installation
```bash
# Update existing projects
dotnet add package CloudContactAI.CCAI.NET --version 1.1.0

# New projects
dotnet new console
dotnet add package CloudContactAI.CCAI.NET --version 1.1.0
dotnet add package DotNetEnv
```

### Quick Test
```csharp
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

Env.Load();
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY")
};

using var ccai = new CCAIClient(config);
var response = await ccai.SMS.SendSingleAsync(
    firstName: "Test",
    lastName: "User", 
    phone: "+15551234567",
    message: "Hello ${FirstName}! CCAI.NET v1.1.0 is working!",
    title: "v1.1.0 Test"
);
```

## 🔗 Webhook Testing Workflow

### 1. Install ngrok
```bash
brew install ngrok
```

### 2. Start webhook server
```bash
cd examples/webhook-server
dotnet run
```

### 3. Create ngrok tunnel
```bash
ngrok http 3000
```

### 4. Configure CCAI Dashboard
- Navigate to Settings → Integration
- Set webhook URLs to: `https://your-ngrok-url.ngrok.io/webhook`

### 5. Test with SMS
```bash
cd examples
dotnet run
```

### 6. Monitor webhook events
Watch real-time webhook events in your webhook server terminal with full request details.

## 📊 Test Results

### ✅ Package Validation
```
🧪 Testing CloudContactAI.CCAI.NET v1.1.0 Package
================================================
✅ Environment variables loaded
✅ CCAI Client initialized successfully  
✅ SMS Service accessible
✅ Account model creation works
🎉 All basic package tests passed!
📦 CloudContactAI.CCAI.NET v1.1.0 is working correctly
```

### ✅ Webhook Testing
```
Received webhook event at /webhook path!
Headers:
  Content-Type: application/json
  User-Agent: Java/14-ea
Body:
{"message":"Hello John! Test message","messageStatus":"SENT","to":"+1XXXYYYZZZZ"}
```

## 🔄 Migration from v1.0.0

**No breaking changes!** Simply update your package reference:

```bash
dotnet remove package CloudContactAI.CCAI.NET
dotnet add package CloudContactAI.CCAI.NET --version 1.1.0
```

All existing code will continue to work without modifications.

## 🐛 Bug Fixes

- Fixed duplicate assembly attribute compilation errors
- Resolved project dependency conflicts
- Fixed environment variable loading in examples
- Corrected webhook server configuration issues

## 📞 Support

- **Documentation**: Updated README.md with comprehensive guides
- **Examples**: Complete working examples in `/examples` directory  
- **Issues**: Report issues on the project repository
- **Testing**: Use the included test suite to validate your setup

## 🎯 What's Next

Future releases may include:
- Enhanced webhook event types
- Additional messaging channels
- Advanced scheduling features
- Improved error handling and retry logic

---

**Happy coding with CloudContactAI.CCAI.NET v1.1.0! 🚀**
