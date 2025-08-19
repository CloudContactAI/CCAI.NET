using System;
using System.Threading.Tasks;
using CCAI.NET;
using CCAI.NET.SMS;

namespace CCAI.NET.Examples;

public class MmsDebug
{
    public static async Task RunAsync()
    {
        var config = new CCAIConfig
        {
            ClientId = "YOUR_CLIENT_ID",
            ApiKey = "YOUR_API_KEY"
        };
        
        using var ccai = new CCAIClient(config);
        
        var imagePath = "/Users/andreas/CCAI.NET/imageNET.jpg";
        var fileName = "imageNET.jpg";
        var contentType = "image/jpeg";
        
        try
        {
            Console.WriteLine("Step 1: Getting signed upload URL...");
            var uploadResponse = await ccai.MMS.GetSignedUploadUrlAsync(fileName, contentType);
            
            Console.WriteLine($"Signed URL: {uploadResponse.SignedS3Url}");
            Console.WriteLine($"File Key: {uploadResponse.FileKey}");
            
            Console.WriteLine("Step 2: Uploading image...");
            var uploadSuccess = await ccai.MMS.UploadImageToSignedUrlAsync(
                uploadResponse.SignedS3Url,
                imagePath,
                contentType
            );
            
            Console.WriteLine($"Upload success: {uploadSuccess}");
            
            if (uploadSuccess)
            {
                Console.WriteLine("Step 3: Sending MMS...");
                var account = new Account
                {
                    FirstName = "Andreas",
                    LastName = "User",
                    Phone = "+14156961732"
                };
                
                var response = await ccai.MMS.SendAsync(
                    uploadResponse.FileKey,
                    new[] { account },
                    "Hello ${FirstName}! Check out this image.",
                    "MMS Debug Test"
                );
                
                Console.WriteLine($"MMS sent! Status: {response.Status}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner error: {ex.InnerException.Message}");
            }
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}