// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CCAI.NET;
using CCAI.NET.SMS;

namespace CCAI.NET.Examples;

/// <summary>
/// Example demonstrating how to use the MMS functionality
/// </summary>
public class MMSExample
{
    private readonly CCAIClient _client;
    
    public MMSExample(string clientId, string apiKey)
    {
        _client = new CCAIClient(new CCAIConfig
        {
            ClientId = clientId,
            ApiKey = apiKey
        });
    }
    
    /// <summary>
    /// Example 1: Complete MMS workflow (get URL, upload image, send MMS)
    /// </summary>
    public async Task SendMMSWithImageAsync()
    {
        // Path to your image file
        var imagePath = "path/to/your/image.jpg";
        var contentType = "image/jpeg";
        
        // Define recipient
        var account = new Account
        {
            FirstName = "John",
            LastName = "Doe",
            Phone = "+15551234567"  // Use E.164 format
        };
        
        // Message content and campaign title
        var message = "Hello ${FirstName}, check out this image!";
        var title = "MMS Campaign Example";
        
        // Define progress tracking
        var progressUpdates = new List<string>();
        var options = new SMSOptions
        {
            OnProgress = status => 
            {
                Console.WriteLine($"Progress: {status}");
                progressUpdates.Add(status);
            }
        };
        
        try
        {
            // Send MMS with image in one step
            var response = await _client.MMS.SendWithImageAsync(
                imagePath,
                contentType,
                new[] { account },
                message,
                title,
                options);
            
            Console.WriteLine($"MMS sent! Campaign ID: {response.CampaignId}");
            Console.WriteLine($"Messages sent: {response.MessagesSent}");
            Console.WriteLine($"Status: {response.Status}");
            
            // Print progress updates
            Console.WriteLine("\nProgress updates:");
            foreach (var update in progressUpdates)
            {
                Console.WriteLine($"- {update}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending MMS: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 2: Step-by-step MMS workflow
    /// </summary>
    public async Task SendMMSStepByStepAsync()
    {
        try
        {
            // Path to your image file
            var imagePath = "path/to/your/image.jpg";
            var fileName = Path.GetFileName(imagePath);
            var contentType = "image/jpeg";
            
            // Step 1: Get a signed URL for uploading
            Console.WriteLine("Getting signed upload URL...");
            var uploadResponse = await _client.MMS.GetSignedUploadUrlAsync(
                fileName,
                contentType);
            
            var signedUrl = uploadResponse.SignedS3Url;
            var fileKey = uploadResponse.FileKey;
            
            Console.WriteLine($"Got signed URL: {signedUrl}");
            Console.WriteLine($"File key: {fileKey}");
            
            // Step 2: Upload the image to the signed URL
            Console.WriteLine("Uploading image...");
            var uploadSuccess = await _client.MMS.UploadImageToSignedUrlAsync(
                signedUrl,
                imagePath,
                contentType);
            
            if (!uploadSuccess)
            {
                Console.WriteLine("Failed to upload image");
                return;
            }
            
            Console.WriteLine("Image uploaded successfully");
            
            // Step 3: Send the MMS with the uploaded image
            Console.WriteLine("Sending MMS...");
            
            // Define recipients
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
            
            // Message content and campaign title
            var message = "Hello ${FirstName}, check out this image!";
            var title = "MMS Campaign Example";
            
            // Send the MMS
            var response = await _client.MMS.SendAsync(
                fileKey,
                accounts,
                message,
                title);
            
            Console.WriteLine($"MMS sent! Campaign ID: {response.CampaignId}");
            Console.WriteLine($"Messages sent: {response.MessagesSent}");
            Console.WriteLine($"Status: {response.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MMS workflow: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Example 3: Send a single MMS
    /// </summary>
    public async Task SendSingleMMSAsync()
    {
        try
        {
            // Define the file key of an already uploaded image
            var pictureFileKey = $"{_client.GetClientId()}/campaign/your-image.jpg";
            
            // Send a single MMS
            var response = await _client.MMS.SendSingleAsync(
                pictureFileKey,
                "John",
                "Doe",
                "+15551234567",
                "Hello ${FirstName}, check out this image!",
                "Single MMS Example");
            
            Console.WriteLine($"MMS sent! Campaign ID: {response.CampaignId}");
            Console.WriteLine($"Status: {response.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending single MMS: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Run all examples
    /// </summary>
    public async Task RunAllExamplesAsync()
    {
        Console.WriteLine("=== Example 1: Complete MMS workflow ===");
        await SendMMSWithImageAsync();
        
        Console.WriteLine("\n=== Example 2: Step-by-step MMS workflow ===");
        await SendMMSStepByStepAsync();
        
        Console.WriteLine("\n=== Example 3: Send a single MMS ===");
        await SendSingleMMSAsync();
    }
}
