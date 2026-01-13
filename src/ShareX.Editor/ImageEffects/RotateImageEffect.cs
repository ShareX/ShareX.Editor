using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects;

public class RotateImageEffect : ImageEffect
{
    private readonly float _angle;
    private readonly string _name;

    public override string Name => _name;

    public override ImageEffectCategory Category => ImageEffectCategory.Rotate;

    public RotateImageEffect(float angle, string name)
    {
        _angle = angle;
        _name = name;
    }

    public override SKBitmap Apply(SKBitmap source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        // For standard 90/180 rotations, we can optimize dimensions
        if (_angle % 90 == 0)
        {
            return RotateOrthogonal(source, (int)_angle);
        }

        // For custom angles, we need to calculate new bounds
        return RotateArbitrary(source, _angle);
    }

    private SKBitmap RotateOrthogonal(SKBitmap source, int angle)
    {
        angle = angle % 360;
        if (angle < 0) angle += 360;

        int width = source.Width;
        int height = source.Height;

        if (angle == 90 || angle == 270)
        {
            (width, height) = (height, width);
        }

        SKBitmap result = new SKBitmap(width, height, source.ColorType, source.AlphaType);
        using (SKCanvas canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);

            if (angle == 90)
            {
                canvas.Translate(width, 0);
                canvas.RotateDegrees(90);
            }
            else if (angle == 180)
            {
                canvas.Translate(width, height);
                canvas.RotateDegrees(180);
            }
            else if (angle == 270)
            {
                canvas.Translate(0, height);
                canvas.RotateDegrees(270);
            }

            canvas.DrawBitmap(source, 0, 0);
        }
        return result;
    }

    private SKBitmap RotateArbitrary(SKBitmap source, float angle)
    {
        // Calculate new bounds
        SKMatrix matrix = SKMatrix.CreateRotationDegrees(angle, source.Width / 2f, source.Height / 2f);
        SKRect rect = new SKRect(0, 0, source.Width, source.Height);
        SKRect mapped = matrix.MapRect(rect);
        
        int newWidth = (int)Math.Ceiling(mapped.Width);
        int newHeight = (int)Math.Ceiling(mapped.Height);

        SKBitmap result = new SKBitmap(newWidth, newHeight, source.ColorType, source.AlphaType);
        using (SKCanvas canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);
            
            // Center the image in new canvas
            canvas.Translate(newWidth / 2f, newHeight / 2f);
            canvas.RotateDegrees(angle);
            canvas.Translate(-source.Width / 2f, -source.Height / 2f);
            
            canvas.DrawBitmap(source, 0, 0);
        }
        return result;
    }

    // Factory methods for convenience
    public static RotateImageEffect Clockwise90 => new RotateImageEffect(90, "Rotate 90° clockwise");
    public static RotateImageEffect CounterClockwise90 => new RotateImageEffect(-90, "Rotate 90° counter clockwise");
    public static RotateImageEffect Rotate180 => new RotateImageEffect(180, "Rotate 180°");
    public static RotateImageEffect Custom(float angle) => new RotateImageEffect(angle, "Rotate custom angle");
}
