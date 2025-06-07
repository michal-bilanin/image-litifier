namespace ImageLitifier.ImageProcessor.Services;

using ErrorOr;

public interface IImageProcessingService
{
    Task<ErrorOr<byte[]>> ProcessImageAsync(byte[] image);
}
