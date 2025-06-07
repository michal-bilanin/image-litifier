namespace ImageLitifier.WebApp.Services;

using ErrorOr;

public interface IBlobsManagement
{
    Task<ErrorOr<string>> UploadFile(string fileName, byte[] file);
    Task<ErrorOr<bool>> FileExists(string fileName);
    Task<ErrorOr<string>> GetFileUrl(string fileName);
}
