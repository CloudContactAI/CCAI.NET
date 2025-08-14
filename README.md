# CCAI.NET

A C# client library for interacting with the [CloudContactAI](https://cloudcontactai.com) API.

## Features

- Send SMS messages to single or multiple recipients
- Send MMS messages with images
- Send Email campaigns to single or multiple recipients
- Upload images to S3 with signed URLs
- Variable substitution in messages
- Manage webhooks for event notifications
- Async/await support
- Progress tracking
- Comprehensive error handling
- Full test coverage

## Requirements

- .NET 8.0 or higher

## Installation

```bash
dotnet add package CloudContactAI.CCAI.NET
dotnet add package DotNetEnv
```

## Configuration

Create a `.env` file in your project root:

```
CCAI_CLIENT_ID=1231
CCAI_API_KEY=your-api-key-here
```

## Usage

### SMS Basic Usage

```csharp
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

// Load environment variables
Env.Load();

// Initialize the client
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
};

using var ccai = new CCAIClient(config);

// Send a single SMS
var response = await ccai.SMS.SendSingleAsync(
    firstName: "John",
    lastName: "Doe",
    phone: "+15551234567",
    message: "Hello ${FirstName}, this is a test message!",
    title: "Test Campaign"
);

Console.WriteLine($"Message sent with ID: {response.Id}");

// Send to multiple recipients
var accounts = new List<Account>
{
    new Account
    {
        FirstName = "John",
        LastName = "Doe",
        Phone = "+15551234567"
    },
    new Account
    {
        FirstName = "Jane",
        LastName = "Smith",
        Phone = "+15559876543"
    }
};

var campaignResponse = await ccai.SMS.SendAsync(
    accounts: accounts,
    message: "Hello ${FirstName} ${LastName}, this is a test message!",
    title: "Bulk Test Campaign"
);

Console.WriteLine($"Campaign sent with ID: {campaignResponse.CampaignId}");
```

### MMS Usage

```csharp
using CCAI.NET;
using CCAI.NET.SMS;
using DotNetEnv;

// Load environment variables
Env.Load();

// Initialize the client
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
};

using var ccai = new CCAIClient(config);

// Create options with progress tracking
var options = new SMSOptions
{
    Timeout = 60,
    OnProgress = status => Console.WriteLine($"Progress: {status}")
};

// Complete MMS workflow (get URL, upload image, send MMS)
var imagePath = "path/to/your/image.jpg";
var contentType = "image/jpeg";

// Define recipient
var account = new Account
{
    FirstName = "John",
    LastName = "Doe",
    Phone = "+15551234567"  // Use E.164 format
};

// Send MMS with image in one step
var response = await ccai.MMS.SendWithImageAsync(
    imagePath: imagePath,
    contentType: contentType,
    accounts: new[] { account },
    message: "Hello ${FirstName}, check out this image!",
    title: "MMS Campaign Example",
    options: options
);

Console.WriteLine($"MMS sent! Campaign ID: {response.CampaignId}");
```

### Email Usage

```csharp
using CCAI.NET;
using CCAI.NET.Email;
using DotNetEnv;

// Load environment variables
Env.Load();

// Initialize the client
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
};

using var ccai = new CCAIClient(config);

// Send a single email
var response = await ccai.Email.SendSingleAsync(
    firstName: "John",
    lastName: "Doe",
    email: "john@example.com",
    subject: "Welcome to Our Service",
    message: "<p>Hello ${FirstName},</p><p>Thank you for signing up!</p>",
    senderEmail: "noreply@yourcompany.com",
    replyEmail: "support@yourcompany.com",
    senderName: "Your Company",
    title: "Welcome Email"
);

Console.WriteLine($"Email sent with ID: {response.Id}");

// Send to multiple recipients
var emailAccounts = new List<EmailAccount>
{
    new EmailAccount
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com"
    },
    new EmailAccount
    {
        FirstName = "Jane",
        LastName = "Smith",
        Email = "jane@example.com"
    }
};

var campaign = new EmailCampaign
{
    Subject = "Monthly Newsletter",
    Title = "July 2025 Newsletter",
    Message = @"
        <h1>Monthly Newsletter - July 2025</h1>
        <p>Hello ${FirstName},</p>
        <p>Here are our updates for this month...</p>
    ",
    SenderEmail = "newsletter@yourcompany.com",
    ReplyEmail = "support@yourcompany.com",
    SenderName = "Your Company Newsletter",
    Accounts = emailAccounts,
    CampaignType = "EMAIL",
    AddToList = "noList",
    ContactInput = "accounts",
    FromType = "single",
    Senders = new List<object>()
};

var campaignResponse = await ccai.Email.SendCampaignAsync(
    campaign: campaign,
    options: new EmailOptions
    {
        OnProgress = status => Console.WriteLine($"Progress: {status}")
    }
);

Console.WriteLine($"Email campaign sent with ID: {campaignResponse.Id}");
```

### Scheduled Email Campaign

```csharp
// Schedule for tomorrow at 10:00 AM
var tomorrow = DateTime.Now.AddDays(1).Date.AddHours(10);

var scheduledCampaign = new EmailCampaign
{
    Subject = "Upcoming Event Reminder",
    Title = "Event Reminder Campaign",
    Message = @"
        <h1>Reminder: Upcoming Event</h1>
        <p>Hello ${FirstName},</p>
        <p>This is a reminder about our upcoming event tomorrow.</p>
    ",
    SenderEmail = "events@yourcompany.com",
    ReplyEmail = "events@yourcompany.com",
    SenderName = "Your Company Events",
    Accounts = emailAccounts,
    ScheduledTimestamp = tomorrow.ToString("o"), // ISO 8601 format
    ScheduledTimezone = "America/New_York"
};

var scheduledResponse = await ccai.Email.SendCampaignAsync(scheduledCampaign);
Console.WriteLine($"Email campaign scheduled with ID: {scheduledResponse.Id}");
```

### Webhook Management

```csharp
using CCAI.NET;
using CCAI.NET.Webhook;
using DotNetEnv;

// Load environment variables
Env.Load();

// Initialize the client
var config = new CCAIConfig
{
    ClientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID") ?? throw new InvalidOperationException("CCAI_CLIENT_ID not found"),
    ApiKey = Environment.GetEnvironmentVariable("CCAI_API_KEY") ?? throw new InvalidOperationException("CCAI_API_KEY not found")
};

using var ccai = new CCAIClient(config);

// Register a webhook
var webhookConfig = new WebhookConfig
{
    Url = "https://your-webhook-endpoint.com/webhook",
    Events = new List<WebhookEventType>
    {
        WebhookEventType.MessageSent,
        WebhookEventType.MessageReceived
    },
    Secret = "your-webhook-secret"
};

var registration = await ccai.Webhook.RegisterAsync(webhookConfig);
Console.WriteLine($"Webhook registered with ID: {registration.Id}");

// List all webhooks
var webhooks = await ccai.Webhook.ListAsync();
foreach (var webhook in webhooks)
{
    Console.WriteLine($"Webhook ID: {webhook.Id}, URL: {webhook.Url}");
}

// Update a webhook
var updatedConfig = new WebhookConfig
{
    Url = "https://your-updated-endpoint.com/webhook",
    Events = new List<WebhookEventType> { WebhookEventType.MessageSent },
    Secret = "your-updated-secret"
};

var updatedWebhook = await ccai.Webhook.UpdateAsync(registration.Id, updatedConfig);

// Delete a webhook
var deleteResponse = await ccai.Webhook.DeleteAsync(registration.Id);
Console.WriteLine($"Webhook deleted: {deleteResponse.Success}");

// Parse a webhook event (in your webhook handler)
public void ProcessWebhookEvent(string json, string signature, string secret)
{
    // Verify the signature
    if (ccai.Webhook.VerifySignature(signature, json, secret))
    {
        // Parse the event
        var webhookEvent = ccai.Webhook.ParseEvent(json);
        
        if (webhookEvent is MessageSentEvent sentEvent)
        {
            Console.WriteLine($"Message sent to: {sentEvent.To}");
        }
        else if (webhookEvent is MessageReceivedEvent receivedEvent)
        {
            Console.WriteLine($"Message received from: {receivedEvent.From}");
        }
    }
    else
    {
        Console.WriteLine("Invalid signature");
    }
}
```

### Step-by-Step MMS Workflow

```csharp
// Step 1: Get a signed URL for uploading
var uploadResponse = await ccai.MMS.GetSignedUploadUrlAsync(
    fileName: "image.jpg",
    fileType: "image/jpeg"
);

var signedUrl = uploadResponse.SignedS3Url;
var fileKey = uploadResponse.FileKey;

// Step 2: Upload the image to the signed URL
var uploadSuccess = await ccai.MMS.UploadImageToSignedUrlAsync(
    signedUrl: signedUrl,
    filePath: "path/to/your/image.jpg",
    contentType: "image/jpeg"
);

if (uploadSuccess)
{
    // Step 3: Send the MMS with the uploaded image
    var response = await ccai.MMS.SendAsync(
        pictureFileKey: fileKey,
        accounts: accounts,
        message: "Hello ${FirstName}, check out this image!",
        title: "MMS Campaign Example"
    );
    
    Console.WriteLine($"MMS sent! Campaign ID: {response.CampaignId}");
}
```

### With Progress Tracking

```csharp
// Create options with progress tracking
var options = new SMSOptions
{
    Timeout = 60,
    Retries = 3,
    OnProgress = status => Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {status}")
};

// Send SMS with progress tracking
var response = await ccai.SMS.SendAsync(
    accounts: accounts,
    message: message,
    title: title,
    options: options
);
```

### Synchronous API

```csharp
// Send a single SMS synchronously
var response = ccai.SMS.SendSingle(
    firstName: "John",
    lastName: "Doe",
    phone: "+15551234567",
    message: "Hello ${FirstName}, this is a test message!",
    title: "Test Campaign"
);

// Send a single MMS synchronously
var mmsResponse = ccai.MMS.SendSingle(
    pictureFileKey: "your-client-id/campaign/image.jpg",
    firstName: "John",
    lastName: "Doe",
    phone: "+15551234567",
    message: "Hello ${FirstName}, check out this image!",
    title: "MMS Campaign"
);

// Send a single email synchronously
var emailResponse = ccai.Email.SendSingle(
    firstName: "John",
    lastName: "Doe",
    email: "john@example.com",
    subject: "Welcome to Our Service",
    message: "<p>Hello ${FirstName},</p><p>Thank you for signing up!</p>",
    senderEmail: "noreply@yourcompany.com",
    replyEmail: "support@yourcompany.com",
    senderName: "Your Company",
    title: "Welcome Email"
);
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Testing Webhook Installation

### Step 1: Install Ngrok

```bash
brew install ngrok
```

### Step 2: Verify Ngrok

```bash
ngrok version
```

### Step 3: Start the standalone webhook server

Open a new terminal window and run:

```bash
cd /Users/../CCAI.NET/examples/webhook-server
dotnet run
```

This will start a webhook server on `http://localhost:3000`

### Step 4: In another terminal, start ngrok

Open another terminal window and run:

```bash
ngrok http 3000
```

This will create a public tunnel to your local webhook server.

If you have not signed up for ngrok, you will need to:

**ERROR:** Sign up for an account: https://dashboard.ngrok.com/signup  
**ERROR:** Install your authtoken: https://dashboard.ngrok.com/get-started/your-authtoken

### Step 5: Get your ngrok URL

ngrok will display something like:
```
Forwarding    https://abc123.ngrok.io -> http://localhost:3000
```

Copy that `https://abc123.ngrok.io` URL - this is your public webhook URL.

Example: `https://81dbae920588.ngrok-free.app`

### Step 6: Configure CCAI with your Ngrok URL

1. Log in to your CCAI account 
2. Navigate to the Settings\Integration tab
3. Specify your ngrok url + '/webhook'

**SMS Callbacks:**
- Call this URL when an inbound message is received: `https://81dbae920588.ngrok-free.app/webhook`
- Call this URL after an outbound message has been delivered: `https://81dbae920588.ngrok-free.app/webhook`

### Step 7: Send a test SMS to trigger webhook

```bash
cd /Users/../CCAI.NET/examples
dotnet run
```

### Step 8: The Web server should receive the delivery notification

Press Ctrl+C to stop the server

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:3000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/j../CCAI.NET/examples/webhook-server
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 POST http://81dbae920588.ngrok-free.app/webhook - application/json 175
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'HTTP: POST /webhook'
Received webhook event at /webhook path!
Headers:
  Accept: application/json, application/*+json
  Host: 81dbae920588.ngrok-free.app
  User-Agent: Java/14-ea
  Accept-Encoding: gzip
  Content-Type: application/json
  Content-Length: 175
  X-Forwarded-For: 157.245.236.180
  X-Forwarded-Host: 81dbae920588.ngrok-free.app
  X-Forwarded-Proto: https
Body:
{"message":"Hello John! We are testing the CCAI SMS functionality with the webhooks","segments":1,"smsSid":141321,"messageStatus":"SENT","totalPrice":0.03,"to":"+1XXXYYYZZZZ"}
info: Microsoft.AspNetCore.Http.Result.OkObjectResult[1]
      Setting HTTP status code 200.
info: Microsoft.AspNetCore.Http.Result.OkObjectResult[3]
      Writing value of type 'String' as Json.
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'HTTP: POST /webhook'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://81dbae920588.ngrok-free.app/webhook - 200 - application/json;+charset=utf-8 137.6141ms
```

### Step 9: From your phone, respond to the message

On your mobile phone, respond to the message that was sent to you by CCAI

### Step 10: Web Server should receive the response notification

```
Received webhook event at /webhook path!
Headers:
  Accept: text/plain, application/json, application/*+json, */*
  Host: 81dbae920588.ngrok-free.app
  User-Agent: Java/14-ea
  Accept-Encoding: gzip
  Content-Type: application/json
  Content-Length: 204
  X-Forwarded-For: 157.245.236.180
  X-Forwarded-Host: 81dbae920588.ngrok-free.app
  X-Forwarded-Proto: https
Body:
{"campaign":{"id":141293,"title":"Default Campaign","message":"","senderPhone":null,"createdAt":"2025-08-13T21:20:50.212623Z","runAt":"null"},"from":"+1XXXYYYZZZZ","to":"+14158735045","message":"Rockin "}
info: Microsoft.AspNetCore.Http.Result.OkObjectResult[1]
      Setting HTTP status code 200.
info: Microsoft.AspNetCore.Http.Result.OkObjectResult[3]
      Writing value of type 'String' as Json.
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'HTTP: POST /webhook'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://81dbae920588.ngrok-free.app/webhook - 200 - application/json;+charset=utf-8 3.7664ms
```

This demonstrates a complete webhook testing workflow where:
1. Outbound SMS messages trigger delivery notifications
2. Inbound SMS responses trigger message received notifications
3. All webhook events are captured and logged by your local webhook server
