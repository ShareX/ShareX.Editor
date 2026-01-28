using SkiaSharp;
using ShareX.Editor.Helpers;
using ShareX.Editor.ImageEffects;
using System.Diagnostics;

namespace ShareX.Editor.Verification;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ShareX.Editor Verification Tool");
        
        string mode = args.Length > 0 ? args[0] : "--generate-baseline";
        string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VerificationOutput");
        Directory.CreateDirectory(outputDir);

        if (mode == "--generate-baseline")
        {
            Console.WriteLine("Generating baseline images...");
            GenerateBaseline(outputDir);
        }
        else if (mode == "--verify")
        {
            Console.WriteLine("Verifying against baseline...");
            string baselineDir = outputDir;
            string newDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VerificationOutput_New");
            Directory.CreateDirectory(newDir);
            
            GenerateBaseline(newDir); // Generate new images using Refactored code (via ImageHelpers proxy)
            
            // Compare
            var baselineFiles = Directory.GetFiles(baselineDir, "*.png");
            bool allMatch = true;
            foreach (var file in baselineFiles)
            {
                string filename = Path.GetFileName(file);
                string newFile = Path.Combine(newDir, filename);
                
                if (!File.Exists(newFile))
                {
                    Console.WriteLine($"[FAIL] Missing file: {filename}");
                    allMatch = false;
                    continue;
                }
                
                byte[] baselineBytes = File.ReadAllBytes(file);
                byte[] newBytes = File.ReadAllBytes(newFile);
                
                if (!baselineBytes.SequenceEqual(newBytes))
                {
                    Console.WriteLine($"[FAIL] Mismatch: {filename}");
                    allMatch = false;
                }
                else
                {
                    Console.WriteLine($"[PASS] {filename}");
                }
            }
            
            if (allMatch) Console.WriteLine("VERIFICATION SUCCESSFUL: All images match.");
            else Console.WriteLine("VERIFICATION FAILED: Some images do not match.");
        }
        else
        {
            Console.WriteLine("Unknown mode. Usage: [--generate-baseline | --verify]");
        }
    }

    static void GenerateBaseline(string outputDir)
    {
        using SKBitmap source = CreateTestPattern(256, 256);
        Save(source, outputDir, "Original.png");

        // Rotate
        Save(ImageHelpers.Rotate90Clockwise(source), outputDir, "Rotate90.png");
        Save(ImageHelpers.Rotate90CounterClockwise(source), outputDir, "RotateMinus90.png");
        Save(ImageHelpers.Rotate180(source), outputDir, "Rotate180.png");

        // Flip
        Save(ImageHelpers.FlipHorizontal(source), outputDir, "FlipHorizontal.png");
        Save(ImageHelpers.FlipVertical(source), outputDir, "FlipVertical.png");

        // Adjustments
        Save(new BrightnessImageEffect { Amount = 20 }.Apply(source), outputDir, "Brightness_20.png");
        Save(new ContrastImageEffect { Amount = 20 }.Apply(source), outputDir, "Contrast_20.png");
        Save(new HueImageEffect { Amount = 45 }.Apply(source), outputDir, "Hue_45.png");
        Save(new SaturationImageEffect { Amount = 50 }.Apply(source), outputDir, "Saturation_50.png");
        Save(new GammaImageEffect { Amount = 1.5f }.Apply(source), outputDir, "Gamma_1.5.png");
        Save(new AlphaImageEffect { Amount = 50 }.Apply(source), outputDir, "Alpha_50.png");
        Save(new ColorizeImageEffect { Color = SKColors.Red, Strength = 50 }.Apply(source), outputDir, "Colorize_Red_50.png");
        
        // Filters
        Save(new InvertImageEffect().Apply(source), outputDir, "Invert.png");
        Save(new GrayscaleImageEffect().Apply(source), outputDir, "Grayscale.png");
        Save(new BlackAndWhiteImageEffect().Apply(source), outputDir, "BlackAndWhite.png");
        Save(new SepiaImageEffect().Apply(source), outputDir, "Sepia.png");
        Save(new PolaroidImageEffect().Apply(source), outputDir, "Polaroid.png");

        Console.WriteLine($"Baseline generation complete. Check: {outputDir}");
    }

    static SKBitmap CreateTestPattern(int width, int height)
    {
        SKBitmap bmp = new SKBitmap(width, height);
        using (SKCanvas canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.White);
            
            // Draw gradient
            using (var paint = new SKPaint())
            {
                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(width, height),
                    new SKColor[] { SKColors.Red, SKColors.Green, SKColors.Blue },
                    null,
                    SKShaderTileMode.Clamp);
                canvas.DrawRect(0, 0, width, height, paint);
            }

            // Draw some shapes
            using (var paint = new SKPaint { Color = SKColors.Yellow, IsAntialias = true })
            {
                canvas.DrawCircle(width / 4, height / 4, width / 8, paint);
            }
            
            using (var paint = new SKPaint { Color = SKColors.Cyan, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 5 })
            {
                canvas.DrawRect(width / 2, height / 2, width / 3, height / 3, paint);
            }

            // Draw text to check orientation
            using (var paint = new SKPaint { Color = SKColors.Black, TextSize = 24, IsAntialias = true })
            {
                canvas.DrawText("TOP LEFT", 10, 30, paint);
            }
        }
        return bmp;
    }

    static void Save(SKBitmap bmp, string dir, string name)
    {
        string path = Path.Combine(dir, name);
        using (var image = SKImage.FromBitmap(bmp))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }
    }
}
