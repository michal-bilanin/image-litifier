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
        using var outputStream = new MemoryStream();
        using var sourceImage = await Image.LoadAsync<Rgba32>(new MemoryStream(sourceImageBytes));
        
        // Resize image if too large for performance
        if (sourceImage.Width > 800 || sourceImage.Height > 600)
        {
            sourceImage.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(Math.Min(800, sourceImage.Width), Math.Min(600, sourceImage.Height)),
                Mode = ResizeMode.Max
            }));
        }

        const int frameCount = 20;
        const int frameDelay = 8;
        
        var frames = new List<Image<Rgba32>>();
        
        try
        {
            for (var frame = 0; frame < frameCount; frame++)
            {
                var frameImage = sourceImage.Clone();
                ApplyFlameEffect(frameImage, frame, frameCount);
                frames.Add(frameImage);
            }

            using var gif = frames[0].Clone();
            gif.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = frameDelay;

            for (var i = 1; i < frames.Count; i++)
            {
                var frame = gif.Frames.AddFrame(frames[i].Frames.RootFrame);
                frame.Metadata.GetGifMetadata().FrameDelay = frameDelay;
            }

            var gifMetadata = gif.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0; // Infinite loop
            
            var encoder = new GifEncoder
            {
                ColorTableMode = GifColorTableMode.Global,
                Quantizer = new OctreeQuantizer(new QuantizerOptions { MaxColors = 256 })
            };

            await gif.SaveAsGifAsync(outputStream, encoder);
            return outputStream.ToArray();
        }
        finally
        {
            foreach (var frame in frames)
            {
                frame.Dispose();
            }
        }
    }

    private static void ApplyFlameEffect(Image<Rgba32> image, int frameIndex, int totalFrames)
    {
        var time = (float)frameIndex / totalFrames;
        var intensity = 0.7f + 0.3f * (float)Math.Sin(time * Math.PI * 4);
        var flameHeight = (int)(image.Height * (0.25f + 0.15f * intensity));
        
        image.Mutate(ctx =>
        {
            // Create flame effect from bottom up
            for (int y = image.Height - flameHeight; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var heightFactor = (float)(image.Height - y) / flameHeight;
                    var xNoise = (float)Math.Sin(x * 0.05f + time * 6.28f) * 0.5f + 0.5f;
                    var yNoise = (float)Math.Sin(y * 0.03f + time * 4.0f) * 0.5f + 0.5f;
                    var turbulence = xNoise * yNoise;
                    
                    var flameIntensity = heightFactor * turbulence * intensity;
                    
                    if (flameIntensity > 0.15f)
                    {
                        var flameColor = GetFlameColor(flameIntensity, heightFactor);
                        var existingPixel = image[x, y];
                        image[x, y] = BlendColors(existingPixel, flameColor);
                    }
                }
            }
            
            ctx.GaussianBlur(0.8f, new Rectangle(0, image.Height - flameHeight, image.Width, flameHeight));
        });
    }

    private static Rgba32 GetFlameColor(float intensity, float heightFactor)
    {
        // Create realistic flame colors: deep red -> orange -> yellow -> white
        byte red, green, blue, alpha;
        
        if (heightFactor > 0.8f) // Top of flame - white/yellow
        {
            red = 255;
            green = (byte)(255 * intensity);
            blue = (byte)(200 * intensity * heightFactor);
            alpha = (byte)(180 * intensity);
        }
        else if (heightFactor > 0.5f) // Middle - orange/yellow
        {
            red = 255;
            green = (byte)(165 + 90 * heightFactor);
            blue = (byte)(50 * heightFactor);
            alpha = (byte)(200 * intensity);
        }
        else // Bottom - deep red/orange
        {
            red = (byte)(200 + 55 * intensity);
            green = (byte)(100 * heightFactor);
            blue = (byte)(20 * heightFactor);
            alpha = (byte)(220 * intensity);
        }
        
        return new Rgba32(red, green, blue, alpha);
    }

    private static Rgba32 BlendColors(Rgba32 baseColor, Rgba32 overlay)
    {
        var alpha = overlay.A / 255f;
        var invAlpha = 1f - alpha;
        
        var r = (byte)Math.Min(255, baseColor.R * invAlpha + overlay.R * alpha);
        var g = (byte)Math.Min(255, baseColor.G * invAlpha + overlay.G * alpha);
        var b = (byte)Math.Min(255, baseColor.B * invAlpha + overlay.B * alpha);
        var a = Math.Max(baseColor.A, overlay.A);
        
        return new Rgba32(r, g, b, a);
    }
}
