using Avalonia.Controls;
using Avalonia.Media;
using SkiaSharp;

namespace ShareX.Editor.Annotations;

/// <summary>
/// Blur annotation - applies blur to the region
/// </summary>
public class BlurAnnotation : BaseEffectAnnotation
{
    public BlurAnnotation()
    {
        ToolType = EditorTool.Blur;
        StrokeColor = "#00000000"; // Transparent border
        StrokeWidth = 0;
        Amount = 10; // Default blur radius
    }

    /// <summary>
    /// Creates the Avalonia visual for this annotation
    /// </summary>
    public Control CreateVisual()
    {
        return new Avalonia.Controls.Shapes.Rectangle
        {
            Stroke = Brushes.Transparent,
            StrokeThickness = StrokeWidth,
            Fill = new SolidColorBrush(Color.Parse("#200000FF")), // Faint blue
            Tag = this
        };
    }

    public override void Render(SKCanvas canvas)
    {
        var rect = GetBounds();

        if (EffectBitmap != null)
        {
            // Draw the pre-calculated blurred image
            canvas.DrawBitmap(EffectBitmap, rect.Left, rect.Top);
        }
        else
        {
            // Fallback: draw translucent placeholder if effect not generated yet
            using var paint = new SKPaint
            {
                Color = new SKColor(128, 128, 128, 77), // Gray with 30% opacity
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(rect, paint);
        }

        // Draw selection border if selected
        if (IsSelected)
        {
            using var selectPaint = new SKPaint
            {
                Color = SKColors.DodgerBlue,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            canvas.DrawRect(rect, selectPaint);
        }
    }

    /// <summary>
    /// Update the internal blurred bitmap based on the source image
    /// </summary>
    /// <param name="source">The full source image (SKBitmap)</param>
    public override void UpdateEffect(SKBitmap source)
    {
        if (source == null) return;

        var rect = GetBounds();
        if (rect.Width <= 0 || rect.Height <= 0) return;

        // Convert to integer bounds
        var skRect = new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);

        // Ensure bounds are valid
        skRect.Intersect(new SKRectI(0, 0, source.Width, source.Height));

        if (skRect.Width <= 0 || skRect.Height <= 0) return;

        // Calculate safe padding for the blur kernel (3 sigma covers 99.7% of Gaussian)
        var blurSigma = Amount;
        var padding = (int)System.Math.Ceiling(blurSigma * 3);

        // Calculate inflated bounds to capture context for edge blurring
        var wantedInflatedRect = new SKRectI(
            skRect.Left - padding,
            skRect.Top - padding,
            skRect.Right + padding,
            skRect.Bottom + padding
        );
        
        // Clamp to source image bounds
        var inflatedRect = wantedInflatedRect;
        inflatedRect.Intersect(new SKRectI(0, 0, source.Width, source.Height));

        if (inflatedRect.Width <= 0 || inflatedRect.Height <= 0) return;

        // Extract the padded region from source
        using var paddedCrop = new SKBitmap(inflatedRect.Width, inflatedRect.Height);
        if (!source.ExtractSubset(paddedCrop, inflatedRect)) return;

        // Step 1: Create a surface for the padded region and extend edges where padding was clipped
        // This ensures the blur has real pixel data at all edges
        using var paddedSurface = SKSurface.Create(new SKImageInfo(wantedInflatedRect.Width, wantedInflatedRect.Height));
        var paddedCanvas = paddedSurface.Canvas;
        
        // Fill the entire padded surface by drawing the cropped bitmap with Clamp shader
        // This extends edge pixels into the padding areas where source image didn't have data
        using var clampShader = paddedCrop.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
        using var fillPaint = new SKPaint { Shader = clampShader };
        
        // Calculate where the actual cropped data should be drawn within the wanted padded area
        float offsetX = inflatedRect.Left - wantedInflatedRect.Left;
        float offsetY = inflatedRect.Top - wantedInflatedRect.Top;
        
        paddedCanvas.Save();
        paddedCanvas.Translate(offsetX, offsetY);
        paddedCanvas.DrawRect(new SKRect(-offsetX, -offsetY, wantedInflatedRect.Width - offsetX, wantedInflatedRect.Height - offsetY), fillPaint);
        paddedCanvas.Restore();

        // Step 2: Apply blur to the entire padded surface
        using var paddedImage = paddedSurface.Snapshot();
        using var paddedBitmap = SKBitmap.FromImage(paddedImage);
        
        using var blurSurface = SKSurface.Create(new SKImageInfo(wantedInflatedRect.Width, wantedInflatedRect.Height));
        var blurCanvas = blurSurface.Canvas;
        
        using var blurPaint = new SKPaint();
        blurPaint.ImageFilter = SKImageFilter.CreateBlur(blurSigma, blurSigma);
        blurCanvas.DrawBitmap(paddedBitmap, 0, 0, blurPaint);
        
        // Step 3: Extract just the center portion (the original annotation bounds)
        using var blurredPaddedImage = blurSurface.Snapshot();
        using var blurredPaddedBitmap = SKBitmap.FromImage(blurredPaddedImage);
        
        // The center region within the padded bitmap corresponds to our original skRect
        var centerRect = new SKRectI(
            padding,  // offset from padded edge to actual content
            padding,
            padding + skRect.Width,
            padding + skRect.Height
        );
        
        // Ensure we don't go out of bounds
        centerRect.Intersect(new SKRectI(0, 0, blurredPaddedBitmap.Width, blurredPaddedBitmap.Height));
        
        if (centerRect.Width <= 0 || centerRect.Height <= 0) return;

        // Extract the final result
        var result = new SKBitmap(skRect.Width, skRect.Height);
        if (!blurredPaddedBitmap.ExtractSubset(result, centerRect))
        {
            result.Dispose();
            return;
        }

        // Store as SKBitmap
        EffectBitmap?.Dispose();
        EffectBitmap = result;
    }
}
