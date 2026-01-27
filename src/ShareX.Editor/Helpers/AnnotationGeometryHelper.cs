using SkiaSharp;

namespace ShareX.Editor.Helpers;

public enum SpeechBalloonTailEdge
{
    Top,
    Right,
    Bottom,
    Left
}

public readonly struct SpeechBalloonTailLayout
{
    public SpeechBalloonTailEdge Edge { get; }
    public SKPoint TailPoint { get; }
    public SKPoint BaseStart { get; }
    public SKPoint BaseEnd { get; }

    public SpeechBalloonTailLayout(SpeechBalloonTailEdge edge, SKPoint tailPoint, SKPoint baseStart, SKPoint baseEnd)
    {
        Edge = edge;
        TailPoint = tailPoint;
        BaseStart = baseStart;
        BaseEnd = baseEnd;
    }
}

public static class AnnotationGeometryHelper
{
    public static SpeechBalloonTailLayout GetSpeechBalloonTailLayout(SKRect rect, SKPoint tailPoint, float cornerRadius, float baseTailWidth)
    {
        var effectiveTail = tailPoint == default
            ? new SKPoint(rect.MidX, rect.Bottom + 30)
            : tailPoint;

        float centerX = rect.MidX;
        float centerY = rect.MidY;

        float dx = effectiveTail.X - centerX;
        float dy = effectiveTail.Y - centerY;

        bool isMoreHorizontal = Math.Abs(dx) > Math.Abs(dy);

        SpeechBalloonTailEdge edge;
        if (isMoreHorizontal)
        {
            edge = dx < 0 ? SpeechBalloonTailEdge.Left : SpeechBalloonTailEdge.Right;
        }
        else
        {
            edge = dy < 0 ? SpeechBalloonTailEdge.Top : SpeechBalloonTailEdge.Bottom;
        }

        float minConnectionMargin = cornerRadius + 5;

        SKPoint baseStart;
        SKPoint baseEnd;

        switch (edge)
        {
            case SpeechBalloonTailEdge.Top:
            case SpeechBalloonTailEdge.Bottom:
                {
                    float minX = rect.Left + minConnectionMargin;
                    float maxX = rect.Right - minConnectionMargin;
                    if (minX > maxX)
                    {
                        minX = maxX = rect.MidX;
                    }

                    float connectionX = Math.Clamp(effectiveTail.X, minX, maxX);
                    float halfTailWidth = baseTailWidth / 2f;
                    float tailStartX = Math.Max(minX, connectionX - halfTailWidth);
                    float tailEndX = Math.Min(maxX, connectionX + halfTailWidth);

                    float y = edge == SpeechBalloonTailEdge.Top ? rect.Top : rect.Bottom;
                    baseStart = new SKPoint(tailStartX, y);
                    baseEnd = new SKPoint(tailEndX, y);
                    break;
                }
            case SpeechBalloonTailEdge.Left:
            case SpeechBalloonTailEdge.Right:
                {
                    float minY = rect.Top + minConnectionMargin;
                    float maxY = rect.Bottom - minConnectionMargin;
                    if (minY > maxY)
                    {
                        minY = maxY = rect.MidY;
                    }

                    float connectionY = Math.Clamp(effectiveTail.Y, minY, maxY);
                    float halfTailWidth = baseTailWidth / 2f;
                    float tailStartY = Math.Max(minY, connectionY - halfTailWidth);
                    float tailEndY = Math.Min(maxY, connectionY + halfTailWidth);

                    float x = edge == SpeechBalloonTailEdge.Left ? rect.Left : rect.Right;
                    baseStart = new SKPoint(x, tailStartY);
                    baseEnd = new SKPoint(x, tailEndY);
                    break;
                }
            default:
                baseStart = new SKPoint(rect.MidX, rect.Bottom);
                baseEnd = new SKPoint(rect.MidX, rect.Bottom);
                break;
        }

        return new SpeechBalloonTailLayout(edge, effectiveTail, baseStart, baseEnd);
    }

    public static List<string> WrapLines(string text, float maxWidth, Func<string, float> measure)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return lines;
        }

        if (maxWidth <= 0)
        {
            lines.Add(text);
            return lines;
        }

        foreach (var paragraph in text.Split('\n'))
        {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentLine = "";

            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                if (measure(testLine) <= maxWidth || string.IsNullOrEmpty(currentLine))
                {
                    currentLine = testLine;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }
        }

        return lines;
    }

    public static bool TryGetArrowPolygonPoints(SKPoint start, SKPoint end, double headSize, out SKPoint[] points)
    {
        points = Array.Empty<SKPoint>();
        var dX = end.X - start.X;
        var dY = end.Y - start.Y;
        var length = Math.Sqrt(dX * dX + dY * dY);
        if (length <= 0.001)
        {
            return false;
        }

        var ux = dX / length;
        var uy = dY / length;
        var perpX = -uy;
        var perpY = ux;

        var enlargedHeadSize = headSize * 1.5;
        var arrowAngle = Math.PI / 5.14;

        var arrowBase = new SKPoint(
            (float)(end.X - enlargedHeadSize * ux),
            (float)(end.Y - enlargedHeadSize * uy));

        var arrowheadBaseWidth = enlargedHeadSize * Math.Tan(arrowAngle);

        var arrowBaseLeft = new SKPoint(
            (float)(arrowBase.X + perpX * arrowheadBaseWidth),
            (float)(arrowBase.Y + perpY * arrowheadBaseWidth));

        var arrowBaseRight = new SKPoint(
            (float)(arrowBase.X - perpX * arrowheadBaseWidth),
            (float)(arrowBase.Y - perpY * arrowheadBaseWidth));

        var shaftEndWidth = enlargedHeadSize * 0.30;

        var shaftEndLeft = new SKPoint(
            (float)(arrowBase.X + perpX * shaftEndWidth),
            (float)(arrowBase.Y + perpY * shaftEndWidth));

        var shaftEndRight = new SKPoint(
            (float)(arrowBase.X - perpX * shaftEndWidth),
            (float)(arrowBase.Y - perpY * shaftEndWidth));

        points = new[]
        {
            start,
            shaftEndLeft,
            arrowBaseLeft,
            end,
            arrowBaseRight,
            shaftEndRight
        };

        return true;
    }
}
