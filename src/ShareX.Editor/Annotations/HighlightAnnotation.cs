using SkiaSharp;

namespace ShareX.Editor.Annotations;

/// <summary>
/// Highlight annotation - translucent color overlay
/// </summary>
public class HighlightAnnotation : BaseEffectAnnotation
{
    public HighlightAnnotation()
    {
        ToolType = EditorTool.Highlighter;
        StrokeColor = "#55FFFF00"; // Default yellow transparent
        StrokeWidth = 0; // No border by default
    }

    public override void Render(SKCanvas canvas)
    {
        var rect = GetBounds();
        using var paint = new SKPaint
        {
            Color = ParseColor(StrokeColor),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        
        canvas.DrawRect(rect, paint);
    }
}
