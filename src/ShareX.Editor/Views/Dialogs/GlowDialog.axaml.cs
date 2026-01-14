using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ShareX.Editor.Helpers;
using SkiaSharp;

namespace ShareX.Editor.Views.Dialogs;

public partial class GlowDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    private SKColor _color = SKColors.Yellow;

    public GlowDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private int GetSize() => (int)(this.FindControl<Slider>("SizeSlider")?.Value ?? 20);
    private float GetStrength() => (float)(this.FindControl<Slider>("StrengthSlider")?.Value ?? 80);
    private int GetOffsetX() => (int)(this.FindControl<Slider>("OffsetXSlider")?.Value ?? 0);
    private int GetOffsetY() => (int)(this.FindControl<Slider>("OffsetYSlider")?.Value ?? 0);

    private void OnValueChanged(object? sender, RoutedEventArgs e) => RaisePreview();

    private void OnColorTextChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = this.FindControl<TextBox>("ColorTextBox");
        var preview = this.FindControl<Border>("ColorPreview");
        if (textBox != null && preview != null)
        {
            try
            {
                var color = Color.Parse(textBox.Text ?? "#FFFF00");
                preview.Background = new SolidColorBrush(color);
                _color = new SKColor(color.R, color.G, color.B, color.A);
                RaisePreview();
            }
            catch { }
        }
    }

    private void RaisePreview()
    {
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyGlow(img, GetSize(), GetStrength(), _color, GetOffsetX(), GetOffsetY()),
            "Glow applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyGlow(img, GetSize(), GetStrength(), _color, GetOffsetX(), GetOffsetY()),
            "Glow applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
