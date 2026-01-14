using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ShareX.Editor.Helpers;
using SkiaSharp;

namespace ShareX.Editor.Views.Dialogs;

public partial class OutlineDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    private SKColor _color = SKColors.Black;

    public OutlineDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private int GetSize() => (int)(this.FindControl<Slider>("SizeSlider")?.Value ?? 3);
    private int GetPadding() => (int)(this.FindControl<Slider>("PaddingSlider")?.Value ?? 0);

    private void OnValueChanged(object? sender, RoutedEventArgs e) => RaisePreview();

    private void OnColorTextChanged(object? sender, TextChangedEventArgs e)
    {
        var textBox = this.FindControl<TextBox>("ColorTextBox");
        var preview = this.FindControl<Border>("ColorPreview");
        if (textBox != null && preview != null)
        {
            try
            {
                var color = Color.Parse(textBox.Text ?? "#000000");
                preview.Background = new SolidColorBrush(color);
                _color = new SKColor(color.R, color.G, color.B, color.A);
                RaisePreview();
            }
            catch { }
        }
    }

    private void RaisePreview()
    {
        var size = GetSize();
        var padding = GetPadding();
        
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyOutline(img, size, padding, _color),
            "Outline applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        var size = GetSize();
        var padding = GetPadding();
        
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyOutline(img, size, padding, _color),
            "Outline applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
