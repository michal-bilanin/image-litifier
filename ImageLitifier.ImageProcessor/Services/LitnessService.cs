namespace ImageLitifier.ImageProcessor.Services;

using ErrorOr;

public class LitnessService : IImageProcessingService
{
    public Task<ErrorOr<byte[]>> ProcessImageAsync(byte[] image)
    {
        throw new NotImplementedException();
    }
}
