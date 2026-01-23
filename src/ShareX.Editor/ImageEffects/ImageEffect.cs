using SkiaSharp;

namespace ShareX.Editor.ImageEffects;

public abstract class ImageEffect
{
    public abstract string Name { get; }

    public abstract ImageEffectCategory Category { get; }

    public bool IsEnabled { get; set; } = true;

    public virtual bool HasParameters => false;

    public virtual string? IconKey => null;

    public abstract SKBitmap Apply(SKBitmap source);

    public virtual ImageEffect Clone()
    {
        return (ImageEffect)MemberwiseClone();
    }
}
