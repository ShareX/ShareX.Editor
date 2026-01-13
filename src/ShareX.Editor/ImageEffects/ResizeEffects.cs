using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects;

public class ResizeImageEffect : ImageEffect
{
    private readonly int _width;
    private readonly int _height;
    private readonly bool _maintainAspectRatio;
    private readonly string _name;

    public override string Name => _name;
    public override ImageEffectCategory Category => ImageEffectCategory.Resize;

    // TODO: Resize logic usually requires parameters. 
    // For now, this class might be instantiated with specific presets or will need UI parameter support.
    // The current ImageHelpers.Resize signature is: Resize(SKBitmap source, int width, int height, bool maintainAspectRatio = false, SKFilterQuality quality = SKFilterQuality.High)
    // The catalog just says "Resize image", which implies a parameterized effect.
    
    public override bool HasParameters => true; 

    public ResizeImageEffect(int width, int height, bool maintainAspectRatio = false)
    {
        _width = width;
        _height = height;
        _maintainAspectRatio = maintainAspectRatio;
        _name = "Resize image";
    }
    
    // Default constructor for catalog discovery
    public ResizeImageEffect()
    {
        _name = "Resize image";
    }

    public override SKBitmap Apply(SKBitmap source)
    {
        // Resize logic moved from ImageHelpers
        if (source is null) throw new ArgumentNullException(nameof(source));
        
        int width = _width > 0 ? _width : source.Width;
        int height = _height > 0 ? _height : source.Height;

        if (width <= 0) width = source.Width;
        if (height <= 0) height = source.Height;

        if (_maintainAspectRatio)
        {
            double sourceAspect = (double)source.Width / source.Height;
            double targetAspect = (double)width / height;

            if (sourceAspect > targetAspect)
            {
                // Source is wider, fit to width
                height = (int)Math.Round(width / sourceAspect);
            }
            else
            {
                // Source is taller, fit to height
                width = (int)Math.Round(height * sourceAspect);
            }
        }
        
        // Use High quality by default or passed parameter? Catalog doesn't specify quality. Use High.
        SKImageInfo info = new SKImageInfo(width, height, source.ColorType, source.AlphaType, source.ColorSpace);
        return source.Resize(info, SKFilterQuality.High);
    }
}

public class AutoCropImageEffect : ImageEffect
{
    private readonly SKColor _color;
    private readonly int _tolerance;
    
    public override string Name => "Auto crop image";
    public override ImageEffectCategory Category => ImageEffectCategory.Resize;

    public AutoCropImageEffect(SKColor color, int tolerance = 0)
    {
        _color = color;
        _tolerance = tolerance;
    }

    public AutoCropImageEffect()
    {
        // Default
    }

    public override SKBitmap Apply(SKBitmap source)
    {
        // AutoCrop logic moved from ImageHelpers
        if (source is null) throw new ArgumentNullException(nameof(source));

        int width = source.Width;
        int height = source.Height;

        int minX = width, minY = height, maxX = 0, maxY = 0;
        bool hasContent = false;

        // Scan all pixels to find content bounds
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SKColor pixel = source.GetPixel(x, y);
                if (!ImageHelpers.ColorsMatch(pixel, _color, _tolerance))
                {
                    hasContent = true;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!hasContent)
        {
            // No content found, return 1x1 transparent bitmap
            return new SKBitmap(1, 1, source.ColorType, source.AlphaType);
        }

        int cropWidth = maxX - minX + 1;
        int cropHeight = maxY - minY + 1;

        return ImageHelpers.Crop(source, minX, minY, cropWidth, cropHeight);
    }
}
