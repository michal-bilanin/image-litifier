using Azure.Storage.Blobs;
using ErrorOr;

namespace ImageLitifier.WebApp.Services;

public class BlobsManagement : IBlobsManagement
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobsManagement()
    {
        _connectionString = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING")
            ?? throw new InvalidOperationException("BLOB_STORAGE_CONNECTION_STRING environment variable is not set.");
        _containerName = Environment.GetEnvironmentVariable("BLOB_STORAGE_CONTAINER_NAME")
            ?? throw new InvalidOperationException("BLOB_STORAGE_CONTAINER_NAME environment variable is not set.");
    }

    public async Task<ErrorOr<string>> UploadFile(string fileName, byte[] file)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);

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

    public async Task<ErrorOr<bool>> FileExists(string fileName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        try
        {
            return (await blobClient.ExistsAsync()).Value;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to check file existence: {ex.Message}");
        }
    }

    public async Task<ErrorOr<string>> GetFileUrl(string fileName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = blobContainerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return Error.Failure(description: "File does not exist.");
        }

        return blobClient.Uri.ToString();
    }
}
