using SkiaSharp;

namespace ShareX.Editor.Annotations;

/// <summary>
/// Speech Balloon annotation with tail
/// </summary>
public class SpeechBalloonAnnotation : Annotation
{
    /// <summary>
    /// Tail point (absolute position)
    /// </summary>
    public SKPoint TailPoint { get; set; }
    
    public string Text { get; set; } = "";
    
    /// <summary>
    /// Background color (hex)
    /// </summary>
    public string FillColor { get; set; } = "#FFFFFFFF"; // White
    
    public SpeechBalloonAnnotation()
    {
        ToolType = EditorTool.SpeechBalloon;
        StrokeWidth = 2;
        StrokeColor = "#FF000000";
    }

    public override void Render(SKCanvas canvas)
    {
        var rect = GetBounds();
        if (rect.Width < 5 || rect.Height < 5) return;

        // Default tail point if not set
        if (TailPoint == default)
        {
            TailPoint = new SKPoint(rect.Right, rect.Bottom + 20);
        }

        using var path = new SKPath();
        
        float radius = 10;
        
        // Start Top-Left
        path.MoveTo(rect.Left + radius, rect.Top);
        
        // Top edge
        path.LineTo(rect.Right - radius, rect.Top);
        path.ArcTo(new SKRect(rect.Right - radius * 2, rect.Top, rect.Right, rect.Top + radius * 2), 270, 90, false);
        
        // Right edge
        path.LineTo(rect.Right, rect.Bottom - radius);
        path.ArcTo(new SKRect(rect.Right - radius * 2, rect.Bottom - radius * 2, rect.Right, rect.Bottom), 0, 90, false);
        
        // Bottom edge (with tail)
        float midBottom = rect.Left + rect.Width / 2;
        float tailBaseWidth = 20;

        // To Tail
        path.LineTo(midBottom + tailBaseWidth / 2, rect.Bottom);
        path.LineTo(TailPoint);
        path.LineTo(midBottom - tailBaseWidth / 2, rect.Bottom);

        // To Left
        path.LineTo(rect.Left + radius, rect.Bottom);
        path.ArcTo(new SKRect(rect.Left, rect.Bottom - radius * 2, rect.Left + radius * 2, rect.Bottom), 90, 90, false);
        
        // Left edge
        path.LineTo(rect.Left, rect.Top + radius);
        path.ArcTo(new SKRect(rect.Left, rect.Top, rect.Left + radius * 2, rect.Top + radius * 2), 180, 90, false);
        
        path.Close();

        // Fill
        using var fillPaint = new SKPaint
        {
            Color = SKColor.Parse(FillColor),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawPath(path, fillPaint);
        
        // Stroke
        using var strokePaint = CreateStrokePaint();
        canvas.DrawPath(path, strokePaint);
    }
    
    public override SKRect GetBounds()
    {
        return new SKRect(
            Math.Min(StartPoint.X, EndPoint.X),
            Math.Min(StartPoint.Y, EndPoint.Y),
            Math.Max(StartPoint.X, EndPoint.X),
            Math.Max(StartPoint.Y, EndPoint.Y));
    }

    public override bool HitTest(SKPoint point, float tolerance = 5)
    {
        var bounds = GetBounds();
        var inflated = SKRect.Inflate(bounds, tolerance, tolerance);
        return inflated.Contains(point);
    }
}
