using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileFixController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public FileFixController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("fix-cv-headers")]
    public async Task<IActionResult> FixCvHeaders()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = "strategistplatformcvfile";
            
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container is public
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var fixedCount = 0;
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                
                // Update blob headers
                var headers = new BlobHttpHeaders
                {
                    ContentType = "application/pdf",
                    ContentDisposition = "inline"
                };
                
                await blobClient.SetHttpHeadersAsync(headers);
                fixedCount++;
            }

            return Ok(new { success = true, message = $"Fixed {fixedCount} CV file(s)", count = fixedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    [HttpPost("fix-profile-headers")]
    public async Task<IActionResult> FixProfileHeaders()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerName = "strategistplatformprofilepictures";
            
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container is public
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var fixedCount = 0;
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                var extension = Path.GetExtension(blobItem.Name).ToLower();
                
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "image/jpeg"
                };

                var headers = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    ContentDisposition = "inline"
                };
                
                await blobClient.SetHttpHeadersAsync(headers);
                fixedCount++;
            }

            return Ok(new { success = true, message = $"Fixed {fixedCount} profile picture(s)", count = fixedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}
