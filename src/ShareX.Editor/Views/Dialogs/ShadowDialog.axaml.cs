using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ShareX.Editor.Helpers;
using SkiaSharp;

namespace ShareX.Editor.Views.Dialogs;

public partial class ShadowDialog : UserControl
{
    public event EventHandler<EffectEventArgs>? PreviewRequested;
    public event EventHandler<EffectEventArgs>? ApplyRequested;
    public event EventHandler? CancelRequested;

    private SKColor _color = SKColors.Black;

    public ShadowDialog()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private float GetOpacity() => (float)(this.FindControl<Slider>("OpacitySlider")?.Value ?? 80);
    private int GetSize() => (int)(this.FindControl<Slider>("SizeSlider")?.Value ?? 20);
    private float GetDarkness() => (float)(this.FindControl<Slider>("DarknessSlider")?.Value ?? 50) / 100f;
    private int GetOffsetX() => (int)(this.FindControl<Slider>("OffsetXSlider")?.Value ?? 5);
    private int GetOffsetY() => (int)(this.FindControl<Slider>("OffsetYSlider")?.Value ?? 5);
    private bool GetAutoResize() => this.FindControl<CheckBox>("AutoResizeCheckBox")?.IsChecked ?? true;

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
        PreviewRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyShadow(img, GetOpacity(), GetSize(), GetDarkness(), _color, GetOffsetX(), GetOffsetY(), GetAutoResize()),
            "Shadow applied"));
    }

    private void OnApplyClick(object? sender, RoutedEventArgs e)
    {
        ApplyRequested?.Invoke(this, new EffectEventArgs(
            img => ImageHelpers.ApplyShadow(img, GetOpacity(), GetSize(), GetDarkness(), _color, GetOffsetX(), GetOffsetY(), GetAutoResize()),
            "Shadow applied"));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }
}
