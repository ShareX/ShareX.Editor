using SkiaSharp;
using ShareX.Editor.Helpers;

namespace ShareX.Editor.ImageEffects;

public abstract class AdjustmentImageEffect : ImageEffect
{
    public override ImageEffectCategory Category => ImageEffectCategory.Adjustments;
    public override bool HasParameters => true;
}

public class BrightnessImageEffect : AdjustmentImageEffect
{
    public override string Name => "Brightness";
    public override string IconKey => "IconSun";
    public float Amount { get; set; } = 0; // -100 to 100

    public override SKBitmap Apply(SKBitmap source) 
    {
        float value = Amount / 100f; 
        float[] matrix = {
            1, 0, 0, 0, value,
            0, 1, 0, 0, value,
            0, 0, 1, 0, value,
            0, 0, 0, 1, 0
        };
        return ImageHelpers.ApplyColorMatrix(source, matrix);
    }
}

public class ContrastImageEffect : AdjustmentImageEffect
{
    public override string Name => "Contrast";
    public override string IconKey => "IconAdjust";
    public float Amount { get; set; } = 0; // -100 to 100

    public override SKBitmap Apply(SKBitmap source) 
    {
        float scale = (100f + Amount) / 100f;
        scale = scale * scale; 
        float shift = 0.5f * (1f - scale);

        float[] matrix = {
            scale, 0, 0, 0, shift,
            0, scale, 0, 0, shift,
            0, 0, scale, 0, shift,
            0, 0, 0, 1, 0
        };
        return ImageHelpers.ApplyColorMatrix(source, matrix);
    }
}

public class HueImageEffect : AdjustmentImageEffect
{
    public override string Name => "Hue";
    public override string IconKey => "IconPalette";
    public float Amount { get; set; } = 0; // -180 to 180

    public override SKBitmap Apply(SKBitmap source) 
    {
        float radians = (float)(Amount * Math.PI / 180.0);
        float c = (float)Math.Cos(radians);
        float s = (float)Math.Sin(radians);

        float[] matrix = {
            0.213f + c * 0.787f - s * 0.213f, 0.715f - c * 0.715f - s * 0.715f, 0.072f - c * 0.072f + s * 0.928f, 0, 0,
            0.213f - c * 0.213f + s * 0.143f, 0.715f + c * 0.285f + s * 0.140f, 0.072f - c * 0.072f - s * 0.283f, 0, 0,
            0.213f - c * 0.213f - s * 0.787f, 0.715f - c * 0.715f + s * 0.715f, 0.072f + c * 0.928f + s * 0.072f, 0, 0,
            0, 0, 0, 1, 0
        };
        return ImageHelpers.ApplyColorMatrix(source, matrix);
    }
}

public class SaturationImageEffect : AdjustmentImageEffect
{
    public override string Name => "Saturation";
    public override string IconKey => "IconFillDrip";
    public float Amount { get; set; } = 0; // -100 to 100

    public override SKBitmap Apply(SKBitmap source) 
    {
        float x = 1f + (Amount / 100f);
        float lumR = 0.3086f;
        float lumG = 0.6094f;
        float lumB = 0.0820f;

        float invSat = 1f - x;

        float r = (invSat * lumR);
        float g = (invSat * lumG);
        float b = (invSat * lumB);

        float[] matrix = {
            r + x, g,     b,     0, 0,
            r,     g + x, b,     0, 0,
            r,     g,     b + x, 0, 0,
            0,     0,     0,     1, 0
        };
        return ImageHelpers.ApplyColorMatrix(source, matrix);
    }
}

public class GammaImageEffect : AdjustmentImageEffect
{
    public override string Name => "Gamma";
    public override string IconKey => "IconWaveSquare";
    public float Amount { get; set; } = 1f;

    public override SKBitmap Apply(SKBitmap source) 
    {
        byte[] table = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            float val = i / 255f;
            float corrected = (float)Math.Pow(val, 1.0 / Amount);
            table[i] = (byte)(Math.Max(0, Math.Min(1, corrected)) * 255);
        }

        using var filter = SKColorFilter.CreateTable(null, table, table, table);
        return ImageHelpers.ApplyColorFilter(source, filter);
    }
}

public class AlphaImageEffect : AdjustmentImageEffect
{
    public override string Name => "Alpha";
    public override string IconKey => "IconEyeDropper";
    public float Amount { get; set; } = 100f; // 0 to 100

    public override SKBitmap Apply(SKBitmap source) 
    {
        float a = Amount / 100f;
        float[] matrix = {
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, a, 0
        };
        return ImageHelpers.ApplyColorMatrix(source, matrix);
    }
}

public class ColorizeImageEffect : AdjustmentImageEffect
{
    public override string Name => "Colorize";
    public override string IconKey => "IconTint";
    public SKColor Color { get; set; } = SKColors.Red; // Default
    public float Strength { get; set; } = 50f;

    public override SKBitmap Apply(SKBitmap source) 
    {
        float strength = Strength;
        if (strength <= 0) return source.Copy();
        if (strength > 100) strength = 100;

        using var paint = new SKPaint();

        var grayscaleMatrix = new float[] {
            0.2126f, 0.7152f, 0.0722f, 0, 0,
            0.2126f, 0.7152f, 0.0722f, 0, 0,
            0.2126f, 0.7152f, 0.0722f, 0, 0,
            0,       0,       0,       1, 0
        };
        using var grayscale = SKColorFilter.CreateColorMatrix(grayscaleMatrix);
        using var tint = SKColorFilter.CreateBlendMode(Color, SKBlendMode.Modulate);
        using var composed = SKColorFilter.CreateCompose(tint, grayscale);

        paint.ColorFilter = composed;
        
        SKBitmap result = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
        using (SKCanvas canvas = new SKCanvas(result))
        {
            canvas.Clear(SKColors.Transparent);
            
            if (strength >= 100)
            {
                canvas.DrawBitmap(source, 0, 0, paint);
            }
            else
            {
                canvas.DrawBitmap(source, 0, 0);
                paint.Color = new SKColor(255, 255, 255, (byte)(255 * (strength / 100f)));
                canvas.DrawBitmap(source, 0, 0, paint);
            }
        }
        return result;
    }
}

public enum SelectiveColorRange
{
    Reds,
    Yellows,
    Greens,
    Cyans,
    Blues,
    Magentas,
    Whites,
    Neutrals,
    Blacks
}

public struct SelectiveColorAdjustment
{
    public float Hue;
    public float Saturation;
    public float Lightness;

    public SelectiveColorAdjustment(float h, float s, float l)
    {
        Hue = h;
        Saturation = s;
        Lightness = l;
    }
}

public class SelectiveColorImageEffect : AdjustmentImageEffect
{
    public override string Name => "Selective Color";
    public override string IconKey => "IconHighlighter";
    
    public Dictionary<SelectiveColorRange, SelectiveColorAdjustment> Adjustments { get; set; } = new();

    public override SKBitmap Apply(SKBitmap source)
    {
        if (Adjustments == null || Adjustments.Count == 0) return source.Copy();

        return ImageHelpers.ApplyPixelOperation(source, (c) =>
        {
            c.ToHsl(out float h, out float s, out float l); 
            
            // Determine range preference: Whites/Blacks/Neutrals first
            SelectiveColorRange? range = null;

            // Simplified HSL range detection matching ImageHelpers legacy logic
            if (l > 80) range = SelectiveColorRange.Whites;
            else if (l < 20) range = SelectiveColorRange.Blacks;
            else if (s < 10) range = SelectiveColorRange.Neutrals;
            else
            {
                // Hue based
                float hDeg = h;
                if (hDeg >= 330 || hDeg <= 30) range = SelectiveColorRange.Reds;
                else if (hDeg >= 30 && hDeg < 90) range = SelectiveColorRange.Yellows;
                else if (hDeg >= 90 && hDeg < 150) range = SelectiveColorRange.Greens;
                else if (hDeg >= 150 && hDeg < 210) range = SelectiveColorRange.Cyans;
                else if (hDeg >= 210 && hDeg < 270) range = SelectiveColorRange.Blues;
                else if (hDeg >= 270 && hDeg < 330) range = SelectiveColorRange.Magentas;
            }

            if (range.HasValue && Adjustments.TryGetValue(range.Value, out var adj))
            {
                if (adj.Hue != 0 || adj.Saturation != 0 || adj.Lightness != 0)
                {
                    h = (h + adj.Hue) % 360;
                    if (h < 0) h += 360;

                    s = Math.Clamp(s + adj.Saturation, 0, 100);
                    l = Math.Clamp(l + adj.Lightness, 0, 100);
                    
                    return SKColor.FromHsl(h, s, l, c.Alpha);
                }
            }

            return c;
        });
    }
}

public class ReplaceColorImageEffect : AdjustmentImageEffect
{
    public override string Name => "Replace Color";
    public override string IconKey => "IconSync";
    public SKColor TargetColor { get; set; }
    public SKColor ReplaceColor { get; set; }
    public float Tolerance { get; set; }

    public override SKBitmap Apply(SKBitmap source) 
    {
         int tol = (int)(Tolerance * 2.55f);
         return ImageHelpers.ApplyPixelOperation(source, (c) =>
         {
             if (ImageHelpers.ColorsMatch(c, TargetColor, tol))
             {
                 return ReplaceColor;
             }
             return c;
         });
    }
}
