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
            ClientId = "2682",
            ApiKey = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJpbmZvQGFsbGNvZGUuY29tIiwiaXNzIjoiY2xvdWRjb250YWN0IiwibmJmIjoxNzE5NDQwMjM2LCJpYXQiOjE3MTk0NDAyMzYsInJvbGUiOiJVU0VSIiwiY2xpZW50SWQiOjI2ODIsImlkIjoyNzY0LCJ0eXBlIjoiQVBJX0tFWSIsImtleV9yYW5kb21faWQiOiI1MGRiOTUzZC1hMjUxLTRmZjMtODI5Yi01NjIyOGRhOGE1YTAifQ.PKVjXYHdjBMum9cTgLzFeY2KIb9b2tjawJ0WXalsb8Bckw1RuxeiYKS1bw5Cc36_Rfmivze0T7r-Zy0PVj2omDLq65io0zkBzIEJRNGDn3gx_AqmBrJ3yGnz9s0WTMr2-F1TFPUByzbj1eSOASIKeI7DGufTA5LDrRclVkz32Oo"
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