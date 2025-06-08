using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ImageLitifier.Contracts;
using ImageLitifier.ImageProcessor.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageLitifier.ImageProcessor
{
    public class ProcessImage
    {
        private readonly ILogger<ProcessImage> _logger;
        private readonly HttpClient _httpClient;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IBlobsManagement _blobsManagement;

        private readonly string _imageProcessedContainerName =
            Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.BlobStorageProcessedContainerName)
            ?? throw new InvalidOperationException(
                "BLOB_STORAGE_PROCESSED_CONTAINER_NAME environment variable is not set.");

        private readonly string _imageRequestsContainerName =
            Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.BlobStorageRequestsContainerName)
            ?? throw new InvalidOperationException(
                "BLOB_STORAGE_REQUESTS_CONTAINER_NAME environment variable is not set.");

        public ProcessImage(ILogger<ProcessImage> logger,
            HttpClient httpClient,
            IImageProcessingService imageProcessingService,
            IBlobsManagement blobsManagement)
        {
            _logger = logger;
            _httpClient = httpClient;
            _imageProcessingService = imageProcessingService;
            _blobsManagement = blobsManagement;
        }

        [Function(nameof(ProcessImage))]
        public async Task Run(
            [ServiceBusTrigger(Constants.EnvironmentVariables.ServiceBusQueueName,
                Connection = Constants.EnvironmentVariables.ServiceBusConnectionString)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation($"Processing message with ID: {message.MessageId}");

            try
            {
                var request = JsonSerializer.Deserialize<ImageProcessingRequest>(message.Body.ToString());

                if (request == null)
                {
                    _logger.LogError("Failed to deserialize message body.");
                    return;
                }

                _logger.LogInformation($"Received processing request for image: {request.FileName}");

                byte[] imageBytes;
                imageBytes = await _httpClient.GetByteArrayAsync(request.SourceImageUrl);

                var processedImageBytesResult = await _imageProcessingService.ProcessImageAsync(imageBytes);
                if (processedImageBytesResult.IsError)
                {
                    _logger.LogError($"Image processing failed: {processedImageBytesResult.FirstError}");
                    await _blobsManagement.RemoveFile(_imageRequestsContainerName, request.FileName);
                    return;
                }

                var uploadResult = await _blobsManagement.UploadFile(
                    _imageProcessedContainerName,
                    request.FileName,
                    processedImageBytesResult.Value);

                if (uploadResult.IsError)
                {
                    _logger.LogError($"Failed to upload processed image: {uploadResult.FirstError}");
                    return;
                }

                _logger.LogInformation($"Image {request.FileName} processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the image.");
            }
            finally
            {
                await messageActions.CompleteMessageAsync(message);
            }
        }
    }
}
