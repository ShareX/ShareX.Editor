using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects;

public class FiltersPixelateImageEffect : FiltersImageEffect
{
    public override string Name => "Pixelate";
    public override string IconKey => "IconGrid";
    public override bool HasParameters => true;
    public int Size { get; set; } = 10;

    public override SKBitmap Apply(SKBitmap source) 
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (Size <= 1) return source.Copy();

        return ImageHelpers.Pixelate(source, Size);
    }
}
