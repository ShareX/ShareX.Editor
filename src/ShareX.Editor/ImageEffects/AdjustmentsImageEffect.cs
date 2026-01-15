using SkiaSharp;

namespace ShareX.Editor.ImageEffects;

public abstract class AdjustmentsImageEffect : ImageEffect
{
    public override ImageEffectCategory Category => ImageEffectCategory.Adjustments;
    public override bool HasParameters => true;
}
