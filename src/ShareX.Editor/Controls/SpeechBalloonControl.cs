#region License Information (GPL v3)

/*
    ShareX.Ava - The Avalonia UI implementation of ShareX
    Copyright (c) 2007-2025 ShareX Team

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
                var formattedText = new FormattedText(
                    Annotation.Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    Annotation.FontSize,
                    new SolidColorBrush(strokeColor)
                );

                var textX = (width - formattedText.Width) / 2;
                var textY = (height - formattedText.Height) / 2;

                context.DrawText(formattedText, new Point(textX, textY));
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

                // Default tail point if not set
                var renderTailPoint = new Point(
                    tailPoint == default ? right : tailPoint.X - Annotation!.StartPoint.X,
                    tailPoint == default ? bottom + 20 : tailPoint.Y - Annotation!.StartPoint.Y
                );

                // Determine which edge the tail should connect to based on tail position
                // Calculate the closest edge to the tail point
                float tailBaseWidth = 20;
                
                // Calculate distances to each edge
                float distToBottom = Math.Abs((float)renderTailPoint.Y - bottom);
                float distToTop = Math.Abs((float)renderTailPoint.Y - top);
                float distToLeft = Math.Abs((float)renderTailPoint.X - left);
                float distToRight = Math.Abs((float)renderTailPoint.X - right);
                
                // Find the minimum distance to determine which edge
                float minDist = Math.Min(Math.Min(distToBottom, distToTop), Math.Min(distToLeft, distToRight));
                
                bool tailOnBottom = minDist == distToBottom;
                bool tailOnTop = minDist == distToTop;
                bool tailOnLeft = minDist == distToLeft;
                bool tailOnRight = minDist == distToRight;

                // Start at top-left after the rounded corner
                ctx.BeginFigure(new Point(left + radius, top), true);

                // Top edge
                if (tailOnTop)
                {
                    float midTop = left + width / 2;
                    ctx.LineTo(new Point(midTop - tailBaseWidth / 2, top));
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(new Point(midTop + tailBaseWidth / 2, top));
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

                // Right edge
                if (tailOnRight)
                {
                    float midRight = top + height / 2;
                    ctx.LineTo(new Point(right, midRight - tailBaseWidth / 2));
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(new Point(right, midRight + tailBaseWidth / 2));
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

                // Bottom edge
                if (tailOnBottom)
                {
                    float midBottom = left + width / 2;
                    ctx.LineTo(new Point(midBottom + tailBaseWidth / 2, bottom));
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(new Point(midBottom - tailBaseWidth / 2, bottom));
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

                // Left edge
                if (tailOnLeft)
                {
                    float midLeft = top + height / 2;
                    ctx.LineTo(new Point(left, midLeft + tailBaseWidth / 2));
                    ctx.LineTo(renderTailPoint);
                    ctx.LineTo(new Point(left, midLeft - tailBaseWidth / 2));
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
