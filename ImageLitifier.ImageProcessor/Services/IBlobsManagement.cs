namespace ImageLitifier.ImageProcessor.Services;

using ErrorOr;

public interface IBlobsManagement
{
    Task<ErrorOr<string>> UploadFile(string containerName, string fileName, byte[] file);
    Task<ErrorOr<Success>> RemoveFile(string containerName, string fileName);
}
