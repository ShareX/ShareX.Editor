using Avalonia.Controls;
using ShareX.Editor.Controls;
using ShareX.Editor.Helpers;
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

    /// <summary>
    /// Optional text content inside the balloon
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Font size for the balloon text
    /// </summary>
    public float FontSize { get; set; } = 20;


    /// <summary>
    /// Background color (hex) - defaults to white for speech balloon
    /// </summary>
    public new string FillColor { get; set; } = "#FFFFFFFF"; // White
    public SpeechBalloonAnnotation()
    {
        ToolType = EditorTool.SpeechBalloon;
        StrokeWidth = 2;
        StrokeColor = "#FF000000";
        FillColor = "#FFFFFFFF"; // Default to white
    }

    /// <summary>
    /// Creates the Avalonia visual for this annotation (SpeechBalloonControl)
    /// </summary>
    public Control CreateVisual()
    {
        var control = new SpeechBalloonControl
        {
            Annotation = this,
            IsHitTestVisible = true,
            Tag = this
        };
        
        if (ShadowEnabled)
        {
            control.Effect = new Avalonia.Media.DropShadowEffect
            {
                OffsetX = 3,
                OffsetY = 3,
                BlurRadius = 4,
                Color = Avalonia.Media.Color.FromArgb(128, 0, 0, 0)
            };
        }
        
        return control;
    }

    public override void Render(SKCanvas canvas)
    {
        var rect = GetBounds();

        // Ensure minimum size for visibility
        const float minSize = 20f;
        float width = Math.Max(rect.Width, minSize);
        float height = Math.Max(rect.Height, minSize);
        if (rect.Width < minSize || rect.Height < minSize)
        {
            rect = new SKRect(rect.Left, rect.Top, rect.Left + width, rect.Top + height);
        }

        float radius = 10;
        float baseTailWidth = 20;
        var layout = AnnotationGeometryHelper.GetSpeechBalloonTailLayout(rect, TailPoint, radius, baseTailWidth);
        TailPoint = layout.TailPoint;

        bool tailOnTop = layout.Edge == SpeechBalloonTailEdge.Top;
        bool tailOnBottom = layout.Edge == SpeechBalloonTailEdge.Bottom;
        bool tailOnLeft = layout.Edge == SpeechBalloonTailEdge.Left;
        bool tailOnRight = layout.Edge == SpeechBalloonTailEdge.Right;

        var renderRect = rect;
        float left = renderRect.Left;
        float top = renderRect.Top;
        float right = renderRect.Right;
        float bottom = renderRect.Bottom;
        var renderTailPoint = layout.TailPoint;

        using var path = new SKPath();

        path.MoveTo(left + radius, top);

        // Top edge
        if (tailOnTop)
        {
            path.LineTo(layout.BaseStart.X, layout.BaseStart.Y);
            path.LineTo(renderTailPoint);
            path.LineTo(layout.BaseEnd.X, layout.BaseEnd.Y);
            path.LineTo(right - radius, top);
        }
        else
        {
            path.LineTo(right - radius, top);
        }

        path.ArcTo(new SKRect(right - radius * 2, top, right, top + radius * 2), 270, 90, false);

        // Right edge
        if (tailOnRight)
        {
            path.LineTo(layout.BaseStart.X, layout.BaseStart.Y);
            path.LineTo(renderTailPoint);
            path.LineTo(layout.BaseEnd.X, layout.BaseEnd.Y);
            path.LineTo(right, bottom - radius);
        }
        else
        {
            path.LineTo(right, bottom - radius);
        }

        path.ArcTo(new SKRect(right - radius * 2, bottom - radius * 2, right, bottom), 0, 90, false);

        // Bottom edge
        if (tailOnBottom)
        {
            path.LineTo(layout.BaseStart.X, layout.BaseStart.Y);
            path.LineTo(renderTailPoint);
            path.LineTo(layout.BaseEnd.X, layout.BaseEnd.Y);
            path.LineTo(left + radius, bottom);
        }
        else
        {
            path.LineTo(left + radius, bottom);
        }

        path.ArcTo(new SKRect(left, bottom - radius * 2, left + radius * 2, bottom), 90, 90, false);

        // Left edge
        if (tailOnLeft)
        {
            path.LineTo(layout.BaseStart.X, layout.BaseStart.Y);
            path.LineTo(renderTailPoint);
            path.LineTo(layout.BaseEnd.X, layout.BaseEnd.Y);
            path.LineTo(left, top + radius);
        }
        else
        {
            path.LineTo(left, top + radius);
        }

        path.ArcTo(new SKRect(left, top, left + radius * 2, top + radius * 2), 180, 90, false);

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

        if (!string.IsNullOrEmpty(Text))
        {
            using var textPaint = new SKPaint
            {
                Color = ParseColor(StrokeColor),
                TextSize = FontSize,
                IsAntialias = true
            };

            var metrics = textPaint.FontMetrics;
            float lineHeight = metrics.Descent - metrics.Ascent;
            float padding = 12f;
            float maxTextWidth = Math.Max(0, renderRect.Width - (padding * 2));
            var lines = AnnotationGeometryHelper.WrapLines(Text, maxTextWidth, textPaint.MeasureText);

            float totalHeight = lines.Count * lineHeight;
            float minY = renderRect.Top + padding - metrics.Ascent;
            float maxY = renderRect.Bottom - padding - metrics.Descent - totalHeight + lineHeight;
            float startY = renderRect.MidY - totalHeight / 2 - metrics.Ascent;
            startY = Math.Clamp(startY, minY, maxY);

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                float lineWidth = textPaint.MeasureText(line);
                float textX = renderRect.MidX - lineWidth / 2;
                textX = Math.Max(renderRect.Left + padding, Math.Min(textX, renderRect.Right - padding - lineWidth));
                float textY = startY + (i * lineHeight);
                canvas.DrawText(line, textX, textY, textPaint);
            }
        }
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
        // Include tail in hit area by expanding to cover the tail point
        bounds = SKRect.Union(bounds, new SKRect(
            TailPoint.X - tolerance,
            TailPoint.Y - tolerance,
            TailPoint.X + tolerance,
            TailPoint.Y + tolerance));
        var inflated = SKRect.Inflate(bounds, tolerance, tolerance);
        return inflated.Contains(point);
    }
}
