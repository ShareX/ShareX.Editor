using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects;

public class FiltersSharpenImageEffect : FiltersImageEffect
{
    public override string Name => "Sharpen";
    public override string IconKey => "IconMagic";
    public override bool HasParameters => true;
    public int Strength { get; set; } = 50;

    public override SKBitmap Apply(SKBitmap source) 
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (Strength <= 0) return source.Copy();

        return ImageHelpers.Sharpen(source, Strength / 100f);
    }
}
