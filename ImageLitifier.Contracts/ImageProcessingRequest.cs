namespace ImageLitifier.Contracts;

public class ImageProcessingRequest
{
    public string RequestId { get; set; }
    public string SourceImageUrl { get; set; }
    public string FileName { get; set; }
    public string ProcessingType { get; set; } // e.g., "resize", "filter", etc.
    public int Width { get; set; } // Optional, for resize requests
    public int Height { get; set; } // Optional, for resize requests
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
}
