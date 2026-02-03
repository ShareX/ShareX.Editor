#region License Information (GPL v3)

/*
    ShareX.Editor - The UI-agnostic Editor library for ShareX
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ShareX.Editor.Helpers;
using SkiaSharp;

namespace ShareX.Editor.Annotations;

/// <summary>
/// Arrow annotation (line with arrowhead)
/// </summary>
public class ArrowAnnotation : Annotation
{
    /// <summary>
    /// Arrow head width is proportional to stroke width for visual balance.
    /// ISSUE-006 fix: Centralized magic number constant.
    /// </summary>
    public const double ArrowHeadWidthMultiplier = 3.0;

    /// <summary>
    /// Arrow head size in pixels
    /// </summary>
    public float ArrowHeadSize { get; set; } = 12;

    public ArrowAnnotation()
    {
        ToolType = EditorTool.Arrow;
    }

    /// <summary>
    /// Creates the Avalonia visual for this annotation
    /// </summary>
    public Control CreateVisual()
    {
        var brush = new SolidColorBrush(Color.Parse(StrokeColor));
        var path = new Avalonia.Controls.Shapes.Path
        {
            Stroke = brush,
            StrokeThickness = StrokeWidth,
            Fill = brush, // Fill arrowhead
            Data = new PathGeometry(),
            Tag = this
        };
        
        if (ShadowEnabled)
        {
            path.Effect = new Avalonia.Media.DropShadowEffect
            {
                OffsetX = 3,
                OffsetY = 3,
                BlurRadius = 4,
                Color = Avalonia.Media.Color.FromArgb(128, 0, 0, 0)
            };
        }
        
        return path;
    }

    /// <summary>
    /// Creates arrow geometry for the Avalonia Path (relocated from EditorView)
    /// </summary>
    public Geometry CreateArrowGeometry(Avalonia.Point start, Avalonia.Point end, double headSize)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            if (AnnotationGeometryHelper.TryGetArrowPolygonPoints(
                    new SKPoint((float)start.X, (float)start.Y),
                    new SKPoint((float)end.X, (float)end.Y),
                    headSize,
                    out var points))
            {
                ctx.BeginFigure(new Avalonia.Point(points[0].X, points[0].Y), true);
                ctx.LineTo(new Avalonia.Point(points[1].X, points[1].Y));
                ctx.LineTo(new Avalonia.Point(points[2].X, points[2].Y));
                ctx.LineTo(new Avalonia.Point(points[3].X, points[3].Y));
                ctx.LineTo(new Avalonia.Point(points[4].X, points[4].Y));
                ctx.LineTo(new Avalonia.Point(points[5].X, points[5].Y));
                ctx.EndFigure(true);
            }
            else
            {
                var radius = 2.0;
                ctx.BeginFigure(new Avalonia.Point(start.X - radius, start.Y), true);
                ctx.ArcTo(new Avalonia.Point(start.X + radius, start.Y), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
                ctx.ArcTo(new Avalonia.Point(start.X - radius, start.Y), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
                ctx.EndFigure(true);
            }
        }
        return geometry;
    }

    public override void Render(SKCanvas canvas)
    {
        using var strokePaint = CreateStrokePaint();
        using var fillPaint = new SKPaint
        {
            Color = ParseColor(StrokeColor),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        var headSize = StrokeWidth * (float)ArrowHeadWidthMultiplier;
        if (AnnotationGeometryHelper.TryGetArrowPolygonPoints(StartPoint, EndPoint, headSize, out var points))
        {
            using var path = new SKPath();
            path.MoveTo(points[0]);
            path.LineTo(points[1]);
            path.LineTo(points[2]);
            path.LineTo(points[3]);
            path.LineTo(points[4]);
            path.LineTo(points[5]);
            path.Close();

            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }
        else
        {
            // Fallback for zero-length arrow
            canvas.DrawLine(StartPoint, EndPoint, strokePaint);
        }
    }

    public override bool HitTest(SKPoint point, float tolerance = 5)
    {
        // Reuse line hit test logic
        var dx = EndPoint.X - StartPoint.X;
        var dy = EndPoint.Y - StartPoint.Y;
        var lineLength = (float)Math.Sqrt(dx * dx + dy * dy);
        if (lineLength < 0.001f) return false;

        var t = Math.Max(0, Math.Min(1,
            ((point.X - StartPoint.X) * (EndPoint.X - StartPoint.X) +
             (point.Y - StartPoint.Y) * (EndPoint.Y - StartPoint.Y)) / (lineLength * lineLength)));

        var projection = new SKPoint(
            StartPoint.X + (float)t * (EndPoint.X - StartPoint.X),
            StartPoint.Y + (float)t * (EndPoint.Y - StartPoint.Y));

        var pdx = point.X - projection.X;
        var pdy = point.Y - projection.Y;
        var distance = (float)Math.Sqrt(pdx * pdx + pdy * pdy);
        return distance <= tolerance;
    }
}
