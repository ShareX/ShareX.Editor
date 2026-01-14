#region License Information (GPL v3)

/*
    ShareX.Editor - The UI-agnostic Editor library for ShareX
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

using SkiaSharp;

namespace ShareX.Editor.Helpers;

/// <summary>
/// Image manipulation utilities for the Editor library.
/// </summary>
public static class ImageHelpers
{
    public static SKBitmap Crop(SKBitmap bitmap, SKRectI rect)
    {
        if (bitmap is null) throw new ArgumentNullException(nameof(bitmap));

        SKRectI bounded = new SKRectI(
            Math.Max(0, rect.Left),
            Math.Max(0, rect.Top),
            Math.Min(bitmap.Width, rect.Right),
            Math.Min(bitmap.Height, rect.Bottom));

        if (bounded.Width <= 0 || bounded.Height <= 0)
        {
            return new SKBitmap();
        }

        SKBitmap subset = new SKBitmap(bounded.Width, bounded.Height);
        return bitmap.ExtractSubset(subset, bounded) ? subset : new SKBitmap();
    }

    public static SKBitmap Crop(SKBitmap bitmap, int x, int y, int width, int height)
    {
        return Crop(bitmap, new SKRectI(x, y, x + width, y + height));
    }

    /// <summary>
    /// Cut out a horizontal or vertical section from the image and join the remaining parts
    /// </summary>
    /// <param name="bitmap">Source bitmap</param>
    /// <param name="startPos">Start position (X for vertical cut, Y for horizontal cut)</param>
    /// <param name="endPos">End position (X for vertical cut, Y for horizontal cut)</param>
    /// <param name="isVertical">True for vertical cut (left-right), false for horizontal cut (top-bottom)</param>
    /// <returns>New bitmap with the section cut out</returns>
    public static SKBitmap CutOut(SKBitmap bitmap, int startPos, int endPos, bool isVertical)
    {
        if (bitmap is null) throw new ArgumentNullException(nameof(bitmap));

        // Ensure start is before end
        if (startPos > endPos)
        {
            (startPos, endPos) = (endPos, startPos);
        }

        if (isVertical)
        {
            // Vertical cut: remove columns from startPos to endPos
            int cutWidth = endPos - startPos;
            if (cutWidth <= 0 || cutWidth >= bitmap.Width)
            {
                return bitmap; // Nothing to cut or invalid range
            }

            int newWidth = bitmap.Width - cutWidth;
            int newHeight = bitmap.Height;

            SKBitmap result = new SKBitmap(newWidth, newHeight);
            using SKCanvas canvas = new SKCanvas(result);

            // Draw left part (0 to startPos)
            if (startPos > 0)
            {
                SKRect srcLeft = new SKRect(0, 0, startPos, bitmap.Height);
                SKRect dstLeft = new SKRect(0, 0, startPos, newHeight);
                canvas.DrawBitmap(bitmap, srcLeft, dstLeft);
            }

            // Draw right part (endPos to width), positioned after left part
            if (endPos < bitmap.Width)
            {
                SKRect srcRight = new SKRect(endPos, 0, bitmap.Width, bitmap.Height);
                SKRect dstRight = new SKRect(startPos, 0, newWidth, newHeight);
                canvas.DrawBitmap(bitmap, srcRight, dstRight);
            }

            return result;
        }
        else
        {
            // Horizontal cut: remove rows from startPos to endPos
            int cutHeight = endPos - startPos;
            if (cutHeight <= 0 || cutHeight >= bitmap.Height)
            {
                return bitmap; // Nothing to cut or invalid range
            }

            int newWidth = bitmap.Width;
            int newHeight = bitmap.Height - cutHeight;

            SKBitmap result = new SKBitmap(newWidth, newHeight);
            using SKCanvas canvas = new SKCanvas(result);

            // Draw top part (0 to startPos)
            if (startPos > 0)
            {
                SKRect srcTop = new SKRect(0, 0, bitmap.Width, startPos);
                SKRect dstTop = new SKRect(0, 0, newWidth, startPos);
                canvas.DrawBitmap(bitmap, srcTop, dstTop);
            }

            // Draw bottom part (endPos to height), positioned after top part
            if (endPos < bitmap.Height)
            {
                SKRect srcBottom = new SKRect(0, endPos, bitmap.Width, bitmap.Height);
                SKRect dstBottom = new SKRect(0, startPos, newWidth, newHeight);
                canvas.DrawBitmap(bitmap, srcBottom, dstBottom);
            }

            return result;
        }
    }

    public static void SaveBitmap(SKBitmap bitmap, string filePath, int quality = 100)
    {
        if (bitmap is null) throw new ArgumentNullException(nameof(bitmap));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);

        SKEncodedImageFormat format = GetEncodedFormat(filePath);
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(format, quality);
        using FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(stream);
    }

    private static SKEncodedImageFormat GetEncodedFormat(string filePath)
    {
        string extension = Path.GetExtension(filePath)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
        return extension switch
        {
            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
            "bmp" => SKEncodedImageFormat.Bmp,
            "gif" => SKEncodedImageFormat.Gif,
            "webp" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Png
        };
    }

    /// <summary>
    /// Resize a bitmap to the specified dimensions with optional aspect ratio preservation
    /// </summary>
    /// <param name="source">Source bitmap to resize</param>
    /// <param name="width">Target width</param>
    /// <param name="height">Target height</param>
    /// <param name="maintainAspectRatio">If true, scales proportionally to fit within width/height</param>
    /// <param name="quality">Filter quality for scaling</param>
    /// <returns>New resized bitmap</returns>
    public static SKBitmap Resize(SKBitmap source, int width, int height, bool maintainAspectRatio = false, SKFilterQuality quality = SKFilterQuality.High)
    {
        // Quality ignored in Effect currently or hardcoded to High
        return new ShareX.Editor.ImageEffects.ResizeImageEffect(width, height, maintainAspectRatio).Apply(source);
    }

    /// <summary>
    /// Rotate bitmap 90 degrees clockwise
    /// </summary>
    public static SKBitmap Rotate90Clockwise(SKBitmap source)
    {
        return ShareX.Editor.ImageEffects.RotateImageEffect.Clockwise90.Apply(source);
    }

    /// <summary>
    /// Rotate bitmap 90 degrees counter-clockwise
    /// </summary>
    public static SKBitmap Rotate90CounterClockwise(SKBitmap source)
    {
        return ShareX.Editor.ImageEffects.RotateImageEffect.CounterClockwise90.Apply(source);
    }

    /// <summary>
    /// Rotate bitmap 180 degrees
    /// </summary>
    public static SKBitmap Rotate180(SKBitmap source)
    {
        return ShareX.Editor.ImageEffects.RotateImageEffect.Rotate180.Apply(source);
    }

    /// <summary>
    /// Flip bitmap horizontally (mirror left-to-right)
    /// </summary>
    /// <summary>
    /// Flip bitmap horizontally (mirror left-to-right)
    /// </summary>
    public static SKBitmap FlipHorizontal(SKBitmap source)
    {
        return ShareX.Editor.ImageEffects.FlipImageEffect.Horizontal.Apply(source);
    }

    /// <summary>
    /// Flip bitmap vertically (mirror top-to-bottom)
    /// </summary>
    /// <summary>
    /// Flip bitmap vertically (mirror top-to-bottom)
    /// </summary>
    public static SKBitmap FlipVertical(SKBitmap source)
    {
        return ShareX.Editor.ImageEffects.FlipImageEffect.Vertical.Apply(source);
    }

    /// <summary>
    /// Resize canvas by adding padding around the image
    /// </summary>
    /// <param name="source">Source bitmap</param>
    /// <param name="left">Left padding in pixels</param>
    /// <param name="top">Top padding in pixels</param>
    /// <param name="right">Right padding in pixels</param>
    /// <param name="bottom">Bottom padding in pixels</param>
    /// <param name="backgroundColor">Background color for the padding</param>
    /// <returns>New bitmap with padding</returns>
    public static SKBitmap ResizeCanvas(SKBitmap source, int left, int top, int right, int bottom, SKColor backgroundColor)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        int newWidth = source.Width + left + right;
        int newHeight = source.Height + top + bottom;

        if (newWidth <= 0 || newHeight <= 0)
        {
            throw new ArgumentException("Canvas size after padding must be greater than 0");
        }

        SKBitmap result = new SKBitmap(newWidth, newHeight, source.ColorType, source.AlphaType);
        using (SKCanvas canvas = new SKCanvas(result))
        {
            canvas.Clear(backgroundColor);
            canvas.DrawBitmap(source, left, top);
        }
        return result;
    }

    /// <summary>
    /// Auto-crops the image by removing edges that match the specified color within tolerance
    /// </summary>
    /// <param name="source">Source bitmap</param>
    /// <param name="color">Color to match for cropping</param>
    /// <param name="tolerance">Color tolerance (0-255)</param>
    /// <returns>Cropped bitmap</returns>
    public static SKBitmap AutoCrop(SKBitmap source, SKColor color, int tolerance = 0)
    {
        return new ShareX.Editor.ImageEffects.AutoCropImageEffect(color, tolerance).Apply(source);
    }

    internal static bool ColorsMatch(SKColor c1, SKColor c2, int tolerance)
    {
        return Math.Abs(c1.Red - c2.Red) <= tolerance &&
               Math.Abs(c1.Green - c2.Green) <= tolerance &&
               Math.Abs(c1.Blue - c2.Blue) <= tolerance &&
               Math.Abs(c1.Alpha - c2.Alpha) <= tolerance;
    }



    internal static SKBitmap ApplyColorMatrix(SKBitmap source, float[] matrix)
    {
        using var filter = SKColorFilter.CreateColorMatrix(matrix);
        return ApplyColorFilter(source, filter);
    }

    internal static SKBitmap ApplyColorFilter(SKBitmap source, SKColorFilter filter)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        SKBitmap result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
        using (SKCanvas canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);
            using (SKPaint paint = new SKPaint())
            {
                paint.ColorFilter = filter;
                canvas.DrawBitmap(source, 0, 0, paint);
            }
        }
        return result;
    }



    internal static SKBitmap ApplyPixelOperation(SKBitmap source, Func<SKColor, SKColor> operation)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        SKBitmap result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);

        // Lock pixels for direct access (fast)
        IntPtr srcPtr = source.GetPixels();
        IntPtr dstPtr = result.GetPixels();

        int count = source.Width * source.Height;

        // Iterate assuming 32-bit (8888) format which is standard for SKBitmap usually
        // Unsafe block would be faster, but let's stick to safe GetPixel/SetPixel if possible 
        // or loop over buffer.
        // For simplicity and safety in this context:

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                SKColor original = source.GetPixel(x, y);
                SKColor modified = operation(original);
                result.SetPixel(x, y, modified);
            }
        }

        return result;
    }

    // ============== FILTERS ==============

    public enum BorderType { Outside, Inside }
    public enum DashStyle { Solid, Dash, Dot, DashDot }

    public static SKBitmap ApplyBorder(SKBitmap source, BorderType type, int size, DashStyle dashStyle, SKColor color)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (size <= 0) return source.Copy();

        int newWidth = type == BorderType.Outside ? source.Width + size * 2 : source.Width;
        int newHeight = type == BorderType.Outside ? source.Height + size * 2 : source.Height;

        SKBitmap result = new SKBitmap(newWidth, newHeight);
        using SKCanvas canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        int offsetX = type == BorderType.Outside ? size : 0;
        int offsetY = type == BorderType.Outside ? size : 0;

        // Draw image
        canvas.DrawBitmap(source, offsetX, offsetY);

        // Draw border
        using SKPaint paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = size,
            IsAntialias = true
        };

        // Set dash effect
        float[] intervals = dashStyle switch
        {
            DashStyle.Dash => new float[] { size * 3, size },
            DashStyle.Dot => new float[] { size, size },
            DashStyle.DashDot => new float[] { size * 3, size, size, size },
            _ => null!
        };
        if (intervals != null)
        {
            paint.PathEffect = SKPathEffect.CreateDash(intervals, 0);
        }

        float halfStroke = size / 2f;
        SKRect borderRect = type == BorderType.Outside
            ? new SKRect(halfStroke, halfStroke, newWidth - halfStroke, newHeight - halfStroke)
            : new SKRect(halfStroke, halfStroke, source.Width - halfStroke, source.Height - halfStroke);

        canvas.DrawRect(borderRect, paint);

        return result;
    }

    public static SKBitmap ApplyOutline(SKBitmap source, int size, int padding, SKColor color)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (size <= 0) return source.Copy();

        int totalExpand = size + padding;
        int newWidth = source.Width + totalExpand * 2;
        int newHeight = source.Height + totalExpand * 2;

        SKBitmap result = new SKBitmap(newWidth, newHeight);
        using SKCanvas canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        // Create outline by drawing image multiple times offset in all directions
        using SKPaint outlinePaint = new SKPaint { ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn) };
        
        // Draw outline copies
        for (int dx = -size; dx <= size; dx++)
        {
            for (int dy = -size; dy <= size; dy++)
            {
                if (dx * dx + dy * dy <= size * size) // Circular outline
                {
                    canvas.DrawBitmap(source, totalExpand + dx, totalExpand + dy, outlinePaint);
                }
            }
        }

        // Draw original image on top
        canvas.DrawBitmap(source, totalExpand, totalExpand);

        return result;
    }

    public static SKBitmap ApplyShadow(SKBitmap source, float opacity, int size, float darkness, SKColor color, int offsetX, int offsetY, bool autoResize)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (size <= 0 && !autoResize) return source.Copy();

        int expandX = autoResize ? Math.Abs(offsetX) + size : 0;
        int expandY = autoResize ? Math.Abs(offsetY) + size : 0;
        int newWidth = source.Width + expandX * 2;
        int newHeight = source.Height + expandY * 2;

        SKBitmap result = new SKBitmap(newWidth, newHeight);
        using SKCanvas canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        int imageX = expandX + (autoResize && offsetX < 0 ? -offsetX : 0);
        int imageY = expandY + (autoResize && offsetY < 0 ? -offsetY : 0);
        int shadowX = imageX + offsetX;
        int shadowY = imageY + offsetY;

        // Create shadow
        SKColor shadowColor = new SKColor(
            (byte)(color.Red * darkness),
            (byte)(color.Green * darkness),
            (byte)(color.Blue * darkness),
            (byte)(255 * opacity / 100f));

        using SKPaint shadowPaint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateBlendMode(shadowColor, SKBlendMode.SrcIn),
            ImageFilter = SKImageFilter.CreateBlur(size / 2f, size / 2f)
        };

        canvas.DrawBitmap(source, shadowX, shadowY, shadowPaint);
        canvas.DrawBitmap(source, imageX, imageY);

        return result;
    }

    public static SKBitmap ApplyGlow(SKBitmap source, int size, float strength, SKColor color, int offsetX, int offsetY)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (size <= 0) return source.Copy();

        int expand = size + Math.Max(Math.Abs(offsetX), Math.Abs(offsetY));
        int newWidth = source.Width + expand * 2;
        int newHeight = source.Height + expand * 2;

        SKBitmap result = new SKBitmap(newWidth, newHeight);
        using SKCanvas canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        SKColor glowColor = color.WithAlpha((byte)(255 * strength / 100f));

        using SKPaint glowPaint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateBlendMode(glowColor, SKBlendMode.SrcIn),
            ImageFilter = SKImageFilter.CreateBlur(size, size)
        };

        // Draw glow
        canvas.DrawBitmap(source, expand + offsetX, expand + offsetY, glowPaint);
        // Draw original
        canvas.DrawBitmap(source, expand, expand);

        return result;
    }

    public static SKBitmap ApplyReflection(SKBitmap source, int percentage, int maxAlpha, int minAlpha, int offset, bool skew, int skewSize)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (percentage <= 0) return source.Copy();

        int reflectionHeight = (int)(source.Height * percentage / 100f);
        int newHeight = source.Height + offset + reflectionHeight;

        SKBitmap result = new SKBitmap(source.Width, newHeight);
        using SKCanvas canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        // Draw original
        canvas.DrawBitmap(source, 0, 0);

        // Create reflection (flipped vertically)
        using SKBitmap flipped = new SKBitmap(source.Width, reflectionHeight);
        using (SKCanvas fc = new SKCanvas(flipped))
        {
            fc.Scale(1, -1, 0, reflectionHeight / 2f);
            fc.DrawBitmap(source, 0, 0);
        }

        // Apply gradient fade
        using SKPaint gradientPaint = new SKPaint();
        var gradient = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, reflectionHeight),
            new SKColor[] { new SKColor(255, 255, 255, (byte)maxAlpha), new SKColor(255, 255, 255, (byte)minAlpha) },
            null,
            SKShaderTileMode.Clamp);
        gradientPaint.Shader = gradient;
        gradientPaint.BlendMode = SKBlendMode.DstIn;

        using SKBitmap reflectionBitmap = new SKBitmap(source.Width, reflectionHeight);
        using (SKCanvas rc = new SKCanvas(reflectionBitmap))
        {
            rc.DrawBitmap(flipped, 0, 0);
            rc.DrawRect(new SKRect(0, 0, source.Width, reflectionHeight), gradientPaint);
        }

        // Apply skew if needed
        if (skew && skewSize > 0)
        {
            canvas.Save();
            canvas.Skew(skewSize / 100f, 0);
        }

        canvas.DrawBitmap(reflectionBitmap, 0, source.Height + offset);

        if (skew && skewSize > 0)
        {
            canvas.Restore();
        }

        return result;
    }

    public static SKBitmap ApplyTornEdge(SKBitmap source, int depth, int range, bool top, bool right, bool bottom, bool left, bool curved)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (depth <= 0 || range <= 0) return source.Copy();

        SKBitmap result = source.Copy();
        using SKCanvas canvas = new SKCanvas(result);

        Random rand = new Random(42); // Fixed seed for reproducible results
        using SKPaint erasePaint = new SKPaint
        {
            Color = SKColors.Transparent,
            BlendMode = SKBlendMode.Src
        };

        void TearEdge(bool horizontal, bool isStart)
        {
            int length = horizontal ? source.Width : source.Height;
            int step = range;
            
            for (int i = 0; i < length; i += step)
            {
                int tearDepth = rand.Next(depth / 2, depth);
                int tearWidth = rand.Next(range / 2, range);
                
                SKRect tearRect;
                if (horizontal)
                {
                    int y = isStart ? 0 : source.Height - tearDepth;
                    tearRect = new SKRect(i, y, Math.Min(i + tearWidth, length), isStart ? tearDepth : source.Height);
                }
                else
                {
                    int x = isStart ? 0 : source.Width - tearDepth;
                    tearRect = new SKRect(x, i, isStart ? tearDepth : source.Width, Math.Min(i + tearWidth, length));
                }

                if (curved)
                {
                    using SKPath path = new SKPath();
                    path.AddOval(tearRect);
                    canvas.DrawPath(path, erasePaint);
                }
                else
                {
                    canvas.DrawRect(tearRect, erasePaint);
                }
            }
        }

        if (top) TearEdge(true, true);
        if (bottom) TearEdge(true, false);
        if (left) TearEdge(false, true);
        if (right) TearEdge(false, false);

        return result;
    }

    public static SKBitmap ApplySlice(SKBitmap source, int minHeight, int maxHeight, int minShift, int maxShift)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (minHeight <= 0 || maxHeight <= minHeight) return source.Copy();

        Random rand = new Random(42);
        int maxAbsShift = Math.Max(Math.Abs(minShift), Math.Abs(maxShift));
        int newWidth = source.Width + maxAbsShift * 2;

        SKBitmap result = new SKBitmap(newWidth, source.Height);
        using SKCanvas canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);

        int y = 0;
        while (y < source.Height)
        {
            int sliceHeight = rand.Next(minHeight, maxHeight + 1);
            sliceHeight = Math.Min(sliceHeight, source.Height - y);
            
            int shift = rand.Next(minShift, maxShift + 1);

            SKRect srcRect = new SKRect(0, y, source.Width, y + sliceHeight);
            SKRect dstRect = new SKRect(maxAbsShift + shift, y, maxAbsShift + shift + source.Width, y + sliceHeight);

            canvas.DrawBitmap(source, srcRect, dstRect);

            y += sliceHeight;
        }

        return result;
    }
}
