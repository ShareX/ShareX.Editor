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
using ShareX.Editor.Annotations;
using ShareX.Editor.Helpers;
using SkiaSharp;

namespace ShareX.Editor.Controls
{
    /// <summary>
    /// Custom control for rendering a speech balloon with a draggable tail using Avalonia Path
    /// </summary>
    public class SpeechBalloonControl : Control
    {
        public static readonly StyledProperty<SpeechBalloonAnnotation?> AnnotationProperty =
            AvaloniaProperty.Register<SpeechBalloonControl, SpeechBalloonAnnotation?>(nameof(Annotation));

        public SpeechBalloonAnnotation? Annotation
        {
            get => GetValue(AnnotationProperty);
            set => SetValue(AnnotationProperty, value);
        }

        static SpeechBalloonControl()
        {
            AffectsRender<SpeechBalloonControl>(AnnotationProperty);
            AffectsMeasure<SpeechBalloonControl>(AnnotationProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (Annotation == null || Bounds.Width <= 0 || Bounds.Height <= 0) return;

            // Get the annotation's bounds
            var annotationBounds = Annotation.GetBounds();
            var width = Math.Max(annotationBounds.Width, 20);
            var height = Math.Max(annotationBounds.Height, 20);

            // Create the speech balloon path using Avalonia geometry
            var geometry = CreateSpeechBalloonGeometry(width, height, Annotation.TailPoint);

            // Parse colors
            var strokeColor = Color.Parse(Annotation.StrokeColor);
            var fillColor = Color.Parse(Annotation.FillColor);

            // Draw fill
            context.DrawGeometry(
                new SolidColorBrush(fillColor),
                null,
                geometry
            );

            // Draw stroke
            context.DrawGeometry(
                null,
                new Pen(new SolidColorBrush(strokeColor), Annotation.StrokeWidth),
                geometry
            );

            // Draw text if present
            if (!string.IsNullOrEmpty(Annotation.Text))
            {
                var typeface = new Typeface(FontFamily.Default);
                var padding = 12;
                var maxTextWidth = Math.Max(0, width - (padding * 2));
                var brush = new SolidColorBrush(strokeColor);

                float Measure(string text)
                {
                    var measureText = new FormattedText(
                        text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        Annotation.FontSize,
                        brush);
                    return (float)measureText.Width;
                }

                var lines = AnnotationGeometryHelper.WrapLines(Annotation.Text, maxTextWidth, Measure);
                if (lines.Count == 0)
                {
                    return;
                }

                var lineHeight = Annotation.FontSize;
                var totalHeight = lines.Count * lineHeight;
                var textStartY = Math.Clamp((height - totalHeight) / 2, padding, Math.Max(padding, height - totalHeight - padding));

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var formattedLine = new FormattedText(
                        line,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        Annotation.FontSize,
                        brush);
                    var textX = Math.Clamp((width - formattedLine.Width) / 2, padding, Math.Max(padding, width - formattedLine.Width - padding));
                    var textY = textStartY + (i * lineHeight);
                    context.DrawText(formattedLine, new Point(textX, textY));
                }
            }
        }

        private Geometry CreateSpeechBalloonGeometry(float width, float height, SKPoint tailPoint)
        {
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                float radius = 10;
                float left = 0;
                float top = 0;
                float right = width;
                float bottom = height;

                var bounds = Annotation!.GetBounds();
                float baseTailWidth = 20;
                var layout = AnnotationGeometryHelper.GetSpeechBalloonTailLayout(bounds, tailPoint, radius, baseTailWidth);
                if (tailPoint == default)
                {
                    Annotation!.TailPoint = layout.TailPoint;
                }

                var renderTailPoint = new Point(
                    layout.TailPoint.X - bounds.Left,
                    layout.TailPoint.Y - bounds.Top
                );
                var renderBaseStart = new Point(
                    layout.BaseStart.X - bounds.Left,
                    layout.BaseStart.Y - bounds.Top
                );
                var renderBaseEnd = new Point(
                    layout.BaseEnd.X - bounds.Left,
                    layout.BaseEnd.Y - bounds.Top
                );

                bool tailOnTop = layout.Edge == SpeechBalloonTailEdge.Top;
                bool tailOnBottom = layout.Edge == SpeechBalloonTailEdge.Bottom;
                bool tailOnLeft = layout.Edge == SpeechBalloonTailEdge.Left;
                bool tailOnRight = layout.Edge == SpeechBalloonTailEdge.Right;

                // Start at top-left after the rounded corner
                ctx.BeginFigure(new Point(left + radius, top), true);

                // Top edge - check if tail connects here
                if (tailOnTop)
                {
                    // Draw to tail start, then to tail point (outside), then to tail end
                    ctx.LineTo(renderBaseStart);
                    ctx.LineTo(renderTailPoint); // Tail point is outside
                    ctx.LineTo(renderBaseEnd);
                    ctx.LineTo(new Point(right - radius, top));
                }
                else
                {
                    ctx.LineTo(new Point(right - radius, top));
                }

                // Top-right corner
                ctx.ArcTo(
                    new Point(right, top + radius),
                    new Size(radius, radius),
                    0,
                    false,
                    SweepDirection.Clockwise
                );

                // Right edge - check if tail connects here
                if (tailOnRight)
                {
                    ctx.LineTo(renderBaseStart);
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(renderBaseEnd);
                    ctx.LineTo(new Point(right, bottom - radius));
                }
                else
                {
                    ctx.LineTo(new Point(right, bottom - radius));
                }

                // Bottom-right corner
                ctx.ArcTo(
                    new Point(right - radius, bottom),
                    new Size(radius, radius),
                    0,
                    false,
                    SweepDirection.Clockwise
                );

                // Bottom edge - check if tail connects here
                if (tailOnBottom)
                {
                    // For bottom edge, draw from right to left, so reverse order
                    ctx.LineTo(renderBaseStart);
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(renderBaseEnd);
                    ctx.LineTo(new Point(left + radius, bottom));
                }
                else
                {
                    ctx.LineTo(new Point(left + radius, bottom));
                }

                // Bottom-left corner
                ctx.ArcTo(
                    new Point(left, bottom - radius),
                    new Size(radius, radius),
                    0,
                    false,
                    SweepDirection.Clockwise
                );

                // Left edge - check if tail connects here
                if (tailOnLeft)
                {
                    // For left edge, draw from bottom to top, so reverse order
                    ctx.LineTo(renderBaseStart);
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(renderBaseEnd);
                    ctx.LineTo(new Point(left, top + radius));
                }
                else
                {
                    ctx.LineTo(new Point(left, top + radius));
                }

                // Top-left corner
                ctx.ArcTo(
                    new Point(left + radius, top),
                    new Size(radius, radius),
                    0,
                    false,
                    SweepDirection.Clockwise
                );

                ctx.EndFigure(true);
            }

            return geometry;
        }
    }
}
