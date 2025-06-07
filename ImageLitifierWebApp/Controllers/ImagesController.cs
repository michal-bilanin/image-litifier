using ImageLitifier.Contracts;
using ImageLitifier.WebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageLitifier.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IBlobsManagement _blobsManagement;
    private readonly IAzureServiceBus _serviceBus;

    private readonly string _imageRequestsContainerName =
        Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.BlobStorageRequestsContainerName)
        ?? throw new InvalidOperationException("BLOB_STORAGE_REQUESTS_CONTAINER_NAME environment variable is not set.");

    private readonly string _imageProcessedContainerName =
        Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.BlobStorageProcessedContainerName)
        ?? throw new InvalidOperationException(
            "BLOB_STORAGE_PROCESSED_CONTAINER_NAME environment variable is not set.");

    private readonly string _serviceBusQueueName =
        Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.ServiceBusQueueName)
        ?? throw new InvalidOperationException("SERVICE_BUS_QUEUE_NAME environment variable is not set.");
    
    public ImagesController(IBlobsManagement blobsManagement, IAzureServiceBus serviceBus)
    {
        _blobsManagement = blobsManagement;
        _serviceBus = serviceBus;
    }

    [HttpPost]
    [Route("upload")]
    public async Task<IActionResult> UploadImage(IFormFile imageFile)
    {
        if (imageFile.Length == 0)
        {
            return BadRequest("No image provided");
        }

        var requestId = Guid.NewGuid().ToString();

        // Upload to blob storage
        byte[] imageBytes;
        using (var memoryStream = new MemoryStream())
        {
            await imageFile.CopyToAsync(memoryStream);
            imageBytes = memoryStream.ToArray();
        }

        var fileName = $"{requestId}_{imageFile.FileName}";
        var imageUrlResult = await _blobsManagement.UploadFile(
            _imageRequestsContainerName,
            fileName,
            imageBytes);

        if (imageUrlResult.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Failed to upload image to blob storage: " + imageUrlResult.FirstError);
        }

        var processingRequest = new ImageProcessingRequest
        {
            RequestId = requestId,
            SourceImageUrl = imageUrlResult.Value,
            FileName = fileName,
            ProcessingType = "FlameGif",
        };

        await _serviceBus.SendMessageAsync(
            processingRequest,
            _serviceBusQueueName);

        var statusUrl = Url.Action("GetStatus", "Images", new { requestId }, Request.Scheme);

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
        var processedImageExistsResponse = await _blobsManagement.FileExists(
            _imageProcessedContainerName,
            $"{requestId}_flames.gif");

        if (processedImageExistsResponse.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Failed to check processed image existence: " + processedImageExistsResponse.FirstError);
        }

        if (!processedImageExistsResponse.Value)
        {
            return Ok(new
            {
                Status = "Processing",
                Message = "Your flame GIF is being created..."
            });
        }

        var imageUrlResponse = await _blobsManagement.GetFileUrl(
            _imageProcessedContainerName,
            $"{requestId}_flames.gif");

        if (imageUrlResponse.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Failed to retrieve processed image URL: " + imageUrlResponse.FirstError);
        }

        return Ok(new
        {
            Status = "Completed",
            ResultUrl = imageUrlResponse.Value
        });
    }
}
