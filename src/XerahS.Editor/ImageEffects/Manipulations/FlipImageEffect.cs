using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects.Manipulations;

public class FlipImageEffect : ImageEffect
{
    public enum FlipDirection { Horizontal, Vertical }

    private FlipDirection _direction;
    private string _name;

    public override string Name => _name;
    public override ImageEffectCategory Category => ImageEffectCategory.Manipulations;

    public FlipDirection Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            _name = _direction == FlipDirection.Horizontal ? "Flip horizontal" : "Flip vertical";
        }
    }

    public FlipImageEffect(FlipDirection direction, string name)
    {
        _direction = direction;
        _name = name;
    }

    public FlipImageEffect()
    {
        _direction = FlipDirection.Horizontal;
        _name = "Flip horizontal";
    }

    public override SKBitmap Apply(SKBitmap source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        
        SKBitmap result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
        using (SKCanvas canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);
            if (_direction == FlipDirection.Horizontal)
            {
                canvas.Scale(-1, 1, source.Width / 2f, source.Height / 2f);
            }
            else
            {
                canvas.Scale(1, -1, source.Width / 2f, source.Height / 2f);
            }
            canvas.DrawBitmap(source, 0, 0);
        }
        return result;
    }

    public static FlipImageEffect Horizontal => new FlipImageEffect(FlipDirection.Horizontal, "Flip horizontal");
    public static FlipImageEffect Vertical => new FlipImageEffect(FlipDirection.Vertical, "Flip vertical");
}

