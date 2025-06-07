using ImageLitifier.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageLitifier.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IBlobsManagement _blobsManagement;
    private readonly IAzureServiceBus _serviceBus;

    [HttpPost]
    [Route("upload")]
    public async Task<IActionResult> UploadImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return BadRequest("No image provided");

        var requestId = Guid.NewGuid().ToString();
        const string containerName = "source-images";

        // Upload to blob storage
        byte[] imageBytes;
        using (var memoryStream = new MemoryStream())
        {
            await imageFile.CopyToAsync(memoryStream);
            imageBytes = memoryStream.ToArray();
        }

        string imageUrl = await _blobsManagement.UploadFile(
            containerName,
            $"{requestId}_{imageFile.FileName}",
            imageBytes,
            _configuration.GetConnectionString("BlobStorage"));

        // Create processing request
        var processingRequest = new ImageProcessingRequest
        {
            RequestId = requestId,
            SourceImageUrl = imageUrl,
            FileName = imageFile.FileName,
            ProcessingType = "FlameGif",
            SubmittedAt = DateTime.UtcNow
        };

        // Send to Service Bus
        await _serviceBus.SendMessageAsync(
            processingRequest,
            "image-processing-queue",
            _configuration.GetConnectionString("ServiceBus"));

        var statusUrl = $"https://{Request.Host}/api/Images/status/{requestId}";

        return Accepted(new
        {
            RequestId = requestId,
            StatusUrl = statusUrl,
            Message = "Image processing started"
        });
    }

    [HttpGet]
    [Route("status/{requestId}")]
    public async Task<IActionResult> GetStatus(string requestId)
    {
        // Check if processed image exists in blob storage
        var processedImageExists = await _blobsManagement.FileExists(
            "processed-images",
            $"{requestId}_flames.gif");

        if (!processedImageExists)
        {
            return Ok(new
            {
                Status = "Processing",
                Message = "Your flame GIF is being created..."
            });
        }


        string imageUrl = await _blobsManagement.GetFileUrl(
            "processed-images",
            $"{requestId}_flames.gif");

        return Ok(new
        {
            Status = "Completed",
            ResultUrl = imageUrl
        });
    }
}
