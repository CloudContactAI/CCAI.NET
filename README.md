# CCAI.NET

A C# client library for interacting with the CloudContactAI API.

## Features

- Send SMS messages to single or multiple recipients
- Send MMS messages with images
- Upload images to S3 with signed URLs
- Variable substitution in messages
- Async/await support
- Progress tracking
- Comprehensive error handling
- Full test coverage

## Requirements

- .NET 8.0 or higher

## Installation

```bash
dotnet add package CloudContactAI.CCAI.NET
```

## Usage

### SMS Basic Usage

```csharp
using CCAI.NET;
using CCAI.NET.SMS;

// Initialize the client
var config = new CCAIConfig
{
    ClientId = "YOUR-CLIENT-ID",
    ApiKey = "YOUR-API-KEY"
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

// Initialize the client
var config = new CCAIConfig
{
    ClientId = "YOUR-CLIENT-ID",
    ApiKey = "YOUR-API-KEY"
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
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
