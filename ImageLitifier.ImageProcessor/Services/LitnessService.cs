using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace ImageLitifier.ImageProcessor.Services;

using ErrorOr;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Extensions.Logging;

public class LitnessService : IImageProcessingService
{
    private readonly ILogger<LitnessService> _logger;

    public LitnessService(ILogger<LitnessService> logger)
    {
        _logger = logger;
    }

    public async Task<ErrorOr<byte[]>> ProcessImageAsync(byte[] image)
    {
        try
        {
            _logger.LogInformation("Starting flame GIF processing for image of size {ImageSize} bytes", image.Length);

            if (image.Length == 0)
            {
                return Error.Validation("Image.Empty", "Image data cannot be null or empty");
            }

            var flameGifBytes = await CreateFlameGifAsync(image);

            _logger.LogInformation("Successfully created flame GIF of size {GifSize} bytes", flameGifBytes.Length);
            return flameGifBytes;
        }
        catch (UnknownImageFormatException ex)
        {
            _logger.LogError(ex, "Invalid image format provided");
            return Error.Validation("Image.InvalidFormat", "The provided image format is not supported");
        }
        catch (InvalidImageContentException ex)
        {
            _logger.LogError(ex, "Invalid image content");
            return Error.Validation("Image.InvalidContent", "The image content is corrupted or invalid");
        }
        catch (OutOfMemoryException ex)
        {
            _logger.LogError(ex, "Image too large to process");
            return Error.Failure("Image.TooLarge", "The image is too large to process");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during image processing");
            return Error.Failure("Image.ProcessingFailed", "An unexpected error occurred during image processing");
        }
    }

    private async Task<byte[]> CreateFlameGifAsync(byte[] sourceImageBytes)
    {
        using var sourceImage = Image.Load<Rgba32>(sourceImageBytes);
        using var flameGif = await Image.LoadAsync<Rgba32>("assets/flame.gif");

        using var outputGif = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);

        var gifMetadata = outputGif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0; // Infinite loop

        outputGif.Frames.RemoveFrame(0);

        for (int frameIndex = 0; frameIndex < flameGif.Frames.Count; frameIndex++)
        {
            using var combinedFrame = sourceImage.CloneAs<Rgba32>();
            var flameFrame = flameGif.Frames[frameIndex];
            using var flameFrameImage = new Image<Rgba32>(flameFrame.Width, flameFrame.Height);

            for (int y = 0; y < flameFrame.Height; y++)
            {
                for (int x = 0; x < flameFrame.Width; x++)
                {
                    flameFrameImage[x, y] = flameFrame[x, y];
                }
            }

            if (flameFrameImage.Width != sourceImage.Width || flameFrameImage.Height != sourceImage.Height)
            {
                flameFrameImage.Mutate(ctx => ctx.Resize(sourceImage.Width, sourceImage.Height));
            }

            combinedFrame.Mutate(ctx => ctx.DrawImage(flameFrameImage, new Point(0, 0), 1.0f));

            var frameMetadata = combinedFrame.Frames.RootFrame.Metadata.GetGifMetadata();
            var originalFrameMetadata = flameFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = originalFrameMetadata.FrameDelay > 0 ? originalFrameMetadata.FrameDelay : 10;

            outputGif.Frames.AddFrame(combinedFrame.Frames.RootFrame);
        }

        using var memoryStream = new MemoryStream();
        await outputGif.SaveAsGifAsync(memoryStream, new GifEncoder
        {
            ColorTableMode = GifColorTableMode.Global,
            Quantizer = new OctreeQuantizer(new QuantizerOptions { MaxColors = 256 })
        });

        return memoryStream.ToArray();
    }
}
