using Azure.Storage.Blobs;

namespace ImageLitifier.ImageProcessor.Services;

using ErrorOr;

public class BlobsManagement : IBlobsManagement
{
    private readonly string _connectionString = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.BlobStorageConnectionString)
        ?? throw new InvalidOperationException("BLOB_STORAGE_CONNECTION_STRING environment variable is not set.");

    public async Task<ErrorOr<string>> UploadFile(string containerName, string fileName, byte[] file)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.CreateIfNotExistsAsync();
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        using var stream = new MemoryStream(file);
        try
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to upload file: {ex.Message}");
        }

        return blobClient.Uri.ToString();
    }
}
