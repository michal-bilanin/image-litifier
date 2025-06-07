namespace ImageLitifier.WebApp.Services;

using ErrorOr;

public interface IBlobsManagement
{
    Task<ErrorOr<string>> UploadFile(string containerName, string fileName, byte[] file);
    Task<ErrorOr<bool>> FileExists(string containerName, string fileName);
    Task<ErrorOr<string>> GetFileUrl(string containerName, string fileName);
}
