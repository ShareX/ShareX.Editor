using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects;

public class FiltersBlurImageEffect : FiltersImageEffect
{
    public override string Name => "Blur";
    public override string IconKey => "IconCloud";
    public override bool HasParameters => true;
    public int Radius { get; set; } = 5;

    public override SKBitmap Apply(SKBitmap source) 
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (Radius <= 0) return source.Copy();

        return ImageHelpers.BoxBlur(source, Radius);
    }
}
